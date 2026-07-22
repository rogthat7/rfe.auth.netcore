
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Interfaces.Shared;
using RFE.Auth.Core.Models.User;
using RFE.Auth.Infrastructure.Repositories.Shared;

namespace RFE.Auth.Infrastructure.Repositories
{
    public class UserRepository: RepositoryBase, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async Task AddNewAuthUser(AuthUser entity, string roleName, string appName = "rfe-auth")
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
           
            const string checkUserSql = @"SELECT ""UserId"" FROM ""AUTH"".""AuthUser"" WHERE ""Username"" = @Username";
            var existingUserId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<Guid?>(checkUserSql, new { Username = entity.Username });
            Guid userId;

            if (existingUserId == null)
            {
                var newUserId = entity.UserId ?? Guid.NewGuid();
                const string sql = @"INSERT INTO ""AUTH"".""AuthUser"" (""UserId"", ""Email"", ""Username"", ""Password"", ""Phone"", ""IsVerified"") VALUES (@UserId, @Email, @Username, @Password, @Phone, @IsVerified) RETURNING ""UserId""";
                userId = await _unitOfWork.DbConnection.QuerySingleAsync<Guid>(sql, new { UserId = newUserId, entity.Email, entity.Username, entity.Password, entity.Phone, entity.IsVerified });
            }
            else
            {
                userId = existingUserId.Value;
                const string updatePwdSql = @"UPDATE ""AUTH"".""AuthUser"" SET ""Password"" = @Password WHERE ""UserId"" = @UserId";
                await _unitOfWork.DbConnection.ExecuteAsync(updatePwdSql, new { Password = entity.Password, UserId = userId });
            }

            // Find the target app ID
            const string findAppSql = @"SELECT ""AppId"" FROM ""AUTH"".""Application"" WHERE ""AppName"" = @AppName";
            var appId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<Guid?>(findAppSql, new { AppName = appName });

            if (appId == null)
            {
                var newAppId = Guid.NewGuid();
                // Fallback: Create app dynamically if not present
                const string insertAppSql = @"INSERT INTO ""AUTH"".""Application"" (""AppId"", ""AppName"", ""DisplayName"") VALUES (@AppId, @AppName, @AppName) RETURNING ""AppId""";
                appId = await _unitOfWork.DbConnection.QuerySingleAsync<Guid>(insertAppSql, new { AppId = newAppId, AppName = appName });
            }

            // Find the role ID by roleName
            const string findRoleSql = @"SELECT ""RoleId"" FROM ""AUTH"".""Roles"" WHERE ""RoleName"" = @RoleName";
            var roleId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<Guid?>(findRoleSql, new { RoleName = roleName });
            if (roleId == null)
            {
                // If the role doesn't exist, fall back to "appUser"
                roleId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<Guid?>(findRoleSql, new { RoleName = "appUser" });
            }

            if (roleId != null)
            {
                const string checkRoleSql = @"SELECT 1 FROM ""AUTH"".""UserRole"" WHERE ""UserId"" = @UserId AND ""RoleId"" = @RoleId AND ""AppId"" = @AppId";
                var hasRole = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(checkRoleSql, new { UserId = userId, RoleId = roleId.Value, AppId = appId.Value });
                if (hasRole == null)
                {
                    // Link user to role for rfe-auth app
                    const string insertUserRoleSql = @"INSERT INTO ""AUTH"".""UserRole"" (""UserRoleId"", ""UserId"", ""RoleId"", ""AppId"") VALUES (@UserRoleId, @UserId, @RoleId, @AppId)";
                    await _unitOfWork.DbConnection.ExecuteAsync(insertUserRoleSql, new { UserRoleId = Guid.NewGuid(), UserId = userId, RoleId = roleId.Value, AppId = appId.Value });
                }
            }

            // Find default permission "modbase" or first available
            const string findPermSql = @"SELECT ""PermissionId"" FROM ""AUTH"".""AppPermission"" WHERE ""PermissionName"" = 'modbase'";
            var permId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<Guid?>(findPermSql);
            if (permId == null)
            {
                const string findAnyPermSql = @"SELECT ""PermissionId"" FROM ""AUTH"".""AppPermission"" LIMIT 1";
                permId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<Guid?>(findAnyPermSql);
            }

            if (permId != null)
            {
                const string checkPermSql = @"SELECT 1 FROM ""AUTH"".""UserAppPermission"" WHERE ""UserId"" = @UserId AND ""AppId"" = @AppId AND ""PermissionId"" = @PermissionId";
                var hasPerm = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(checkPermSql, new { UserId = userId, AppId = appId.Value, PermissionId = permId.Value });
                if (hasPerm == null)
                {
                    // Insert User App Permission mapping
                    const string insertUserAppPermSql = @"INSERT INTO ""AUTH"".""UserAppPermission"" (""UAPId"", ""UserId"", ""AppId"", ""PermissionId"") VALUES (@UAPId, @UserId, @AppId, @PermissionId)";
                    await _unitOfWork.DbConnection.ExecuteAsync(insertUserAppPermSql, new { UAPId = Guid.NewGuid(), UserId = userId, AppId = appId.Value, PermissionId = permId.Value });
                }
            }
        }

        public async Task MarkUserAsVerified(string username)
        {
            const string sql = @"UPDATE ""AUTH"".""AuthUser"" SET ""IsVerified"" = TRUE WHERE ""Username"" = @Username";
            await _unitOfWork.DbConnection.ExecuteAsync(sql, new { Username = username });
        }

        public async Task<List<AuthUserByIdGetResponse>> All()
        {
            const string sql = @"
                SELECT 
                    usr.""UserId"", 
                    usr.""Username"", 
                    usr.""Email"", 
                    usr.""Phone"",
                    (
                        SELECT r.""RoleName"" 
                        FROM ""AUTH"".""UserRole"" ur 
                        INNER JOIN ""AUTH"".""Roles"" r ON ur.""RoleId"" = r.""RoleId"" 
                        WHERE ur.""UserId"" = usr.""UserId"" 
                        LIMIT 1
                    ) AS ""Role"",
                    (
                        SELECT string_agg(DISTINCT app.""AppName"", ',') 
                        FROM ""AUTH"".""UserAppPermission"" uap 
                        INNER JOIN ""AUTH"".""Application"" app ON uap.""AppId"" = app.""AppId"" 
                        WHERE uap.""UserId"" = usr.""UserId""
                    ) AS ""Apps""
                FROM ""AUTH"".""AuthUser"" usr";
            var res = await _unitOfWork.DbConnection.QueryAsync<AuthUserByIdGetResponse>(sql);
            return res.ToList();
        }

        public async Task<AuthUser> AuthenticateAuthUser(string username, string password)
        {
            const string sql = @"SELECT ""UserId"", ""Username"", ""Email"", ""Phone"", ""Password"" FROM ""AUTH"".""AuthUser"" WHERE ""Username"" = @Username AND ""Password"" = @Password";
            var res = await _unitOfWork.DbConnection.QueryAsync<AuthUser>(sql, new { Username = username, Password = password });
            return res.FirstOrDefault();
        }

        public async Task<bool> DeleteById(Guid? id)
        {
            if (id == null)
                return false;
            const string sql = @"DELETE FROM ""AUTH"".""AuthUser"" WHERE ""UserId"" = @Id";
            var res = await _unitOfWork.DbConnection.ExecuteAsync(sql, new { Id = id });
            return res > 0;
        }

        public async Task<AuthUserByIdGetResponse> GetById(Guid? id)
        {
            if(id == null)
                return null;
            const string sql = @"SELECT ""UserId"", ""Username"", ""Email"", ""Phone"" FROM ""AUTH"".""AuthUser"" WHERE ""UserId"" = @Id";
            var res = await _unitOfWork.DbConnection.QueryAsync<AuthUserByIdGetResponse>(sql, new { Id = id });
            return res.FirstOrDefault();
        }

        public async Task<bool> Upsert(AuthUser entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            const string sql = @"UPDATE ""AUTH"".""AuthUser"" SET ""Email"" = @Email, ""Phone"" = @Phone WHERE ""Username"" = @Username";
            var res = await _unitOfWork.DbConnection.ExecuteAsync(sql, new { entity.Email, entity.Username, entity.Phone });
            return res > 0;
        }
    }
}