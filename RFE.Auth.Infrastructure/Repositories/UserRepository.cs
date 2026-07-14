
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
           
            const string sql = @"INSERT INTO ""AUTH"".""AuthUser"" (""Email"", ""Username"", ""Password"", ""Phone"") VALUES (@Email, @Username, @Password, @Phone) RETURNING ""UserId""";
            var userId = await _unitOfWork.DbConnection.QuerySingleAsync<int>(sql, new { entity.Email, entity.Username, entity.Password, entity.Phone });

            // Find the target app ID
            const string findAppSql = @"SELECT ""AppId"" FROM ""AUTH"".""Application"" WHERE ""AppName"" = @AppName";
            var appId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(findAppSql, new { AppName = appName });

            if (appId == null)
            {
                // Fallback: Create app dynamically if not present
                const string insertAppSql = @"INSERT INTO ""AUTH"".""Application"" (""AppName"", ""DisplayName"") VALUES (@AppName, @AppName) RETURNING ""AppId""";
                appId = await _unitOfWork.DbConnection.QuerySingleAsync<int>(insertAppSql, new { AppName = appName });
            }

            // Find the role ID by roleName
            const string findRoleSql = @"SELECT ""RoleId"" FROM ""AUTH"".""Roles"" WHERE ""RoleName"" = @RoleName";
            var roleId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(findRoleSql, new { RoleName = roleName });
            if (roleId == null)
            {
                // If the role doesn't exist, fall back to "appUser"
                roleId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(findRoleSql, new { RoleName = "appUser" });
            }

            if (roleId != null)
            {
                // Link user to role for rfe-auth app
                const string insertUserRoleSql = @"INSERT INTO ""AUTH"".""UserRole"" (""UserId"", ""RoleId"", ""AppId"") VALUES (@UserId, @RoleId, @AppId)";
                await _unitOfWork.DbConnection.ExecuteAsync(insertUserRoleSql, new { UserId = userId, RoleId = roleId.Value, AppId = appId.Value });
            }

            // Find default permission "modbase" or first available
            const string findPermSql = @"SELECT ""PermissionId"" FROM ""AUTH"".""AppPermission"" WHERE ""PermissionName"" = 'modbase'";
            var permId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(findPermSql);
            if (permId == null)
            {
                const string findAnyPermSql = @"SELECT ""PermissionId"" FROM ""AUTH"".""AppPermission"" LIMIT 1";
                permId = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(findAnyPermSql);
            }

            if (permId != null)
            {
                // Insert User App Permission mapping
                const string insertUserAppPermSql = @"INSERT INTO ""AUTH"".""UserAppPermission"" (""UserId"", ""AppId"", ""PermissionId"") VALUES (@UserId, @AppId, @PermissionId)";
                await _unitOfWork.DbConnection.ExecuteAsync(insertUserAppPermSql, new { UserId = userId, AppId = appId.Value, PermissionId = permId.Value });
            }
        }

        public async Task MarkUserAsVerified(string username)
        {
            const string sql = @"UPDATE ""AUTH"".""AuthUser"" SET ""IsVerified"" = TRUE WHERE ""Username"" = @Username";
            await _unitOfWork.DbConnection.ExecuteAsync(sql, new { Username = username });
        }

        public async Task<List<AuthUserByIdGetResponse>> All()
        {
            const string sql = @"SELECT ""UserId"", ""Username"", ""Email"", ""Phone"" FROM ""AUTH"".""AuthUser""";
            var res = await _unitOfWork.DbConnection.QueryAsync<AuthUserByIdGetResponse>(sql);
            return res.ToList();
        }

        public async Task<AuthUser> AuthenticateAuthUser(string username, string password)
        {
            const string sql = @"SELECT ""UserId"", ""Username"", ""Email"", ""Phone"", ""Password"" FROM ""AUTH"".""AuthUser"" WHERE ""Username"" = @Username AND ""Password"" = @Password";
            var res = await _unitOfWork.DbConnection.QueryAsync<AuthUser>(sql, new { Username = username, Password = password });
            return res.FirstOrDefault();
        }

        public async Task<bool> DeleteById(int? id)
        {
            if (id == null)
                return false;
            const string sql = @"DELETE FROM ""AUTH"".""AuthUser"" WHERE ""UserId"" = @Id";
            var res = await _unitOfWork.DbConnection.ExecuteAsync(sql, new { Id = id });
            return res > 0;
        }

        public async Task<AuthUserByIdGetResponse> GetById(int? id)
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