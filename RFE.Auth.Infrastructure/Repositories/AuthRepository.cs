
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
    public class AuthRepository: RepositoryBase, IAuthRepository
    {
        public AuthRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async Task<AuthUser> AuthenticateAuthUser(string username, string password)
        {
            long? phoneVal = null;
            if (long.TryParse(username, out long parsedPhone))
            {
                phoneVal = parsedPhone;
            }

            const string sql = @"
                SELECT ""UserId"", ""Username"", ""Email"", ""Phone"", ""Password"", ""IsVerified"" 
                FROM ""AUTH"".""AuthUser"" 
                WHERE (""Username"" = @Username 
                   OR ""Email"" = @Username 
                   OR (@PhoneVal IS NOT NULL AND ""Phone"" = @PhoneVal)) 
                  AND ""Password"" = @Password";
            var res = await _unitOfWork.DbConnection.QueryAsync<AuthUser>(sql, new { Username = username, PhoneVal = phoneVal, Password = password });
            return res.FirstOrDefault();
        }

        public async Task<List<UserAppPermissionResponse>> GetUserAppPermissionsByUserId(Guid? userId)
        {
            const string sql = @"
                SELECT 
                    uap.""UAPId"", 
                    app.""AppName"", 
                    usr.""Username"", 
                    perm.""PermissionName"", 
                    perm.""PermissionType""
                FROM ""AUTH"".""UserAppPermission"" uap
                INNER JOIN ""AUTH"".""Application"" app ON uap.""AppId"" = app.""AppId""
                INNER JOIN ""AUTH"".""AuthUser"" usr ON uap.""UserId"" = usr.""UserId""
                INNER JOIN ""AUTH"".""AppPermission"" perm ON uap.""PermissionId"" = perm.""PermissionId""
                WHERE uap.""UserId"" = @UserId";
            var res = await _unitOfWork.DbConnection.QueryAsync<UserAppPermissionResponse>(sql, new { UserId = userId });
            return res.ToList();
        }

        public async Task<string?> GetUserRoleByUserId(Guid userId)
        {
            const string sql = @"
                SELECT r.""RoleName"" 
                FROM ""AUTH"".""UserRole"" ur
                INNER JOIN ""AUTH"".""Roles"" r ON ur.""RoleId"" = r.""RoleId""
                WHERE ur.""UserId"" = @UserId 
                LIMIT 1";
            return await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<string>(sql, new { UserId = userId });
        }
    }
}