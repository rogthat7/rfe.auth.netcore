
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Models.User;
using RFE.Auth.Infrastructure.Repositories.Shared;

namespace RFE.Auth.Infrastructure.Repositories
{
    public class AuthRepository: RepositoryBase, IAuthRepository
    {

        private const string SprAuthenticataAuthUser = "AUTH.spr_AuthenticataAuthUser";
        private const string SprGetUserAppPermissions = "AUTH.spr_GetUserAppPermissions";

        public AuthRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }
        public async Task<AuthUser> AuthenticateAuthUser(string username, string password)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Username", username, dbType: DbType.String);
            parameters.Add("@Password", password, dbType: DbType.String);
            var res = await ExecuteStoredProcedureListResult<AuthUser>(SprAuthenticataAuthUser, parameters);
            if (res.Response.Count()<=0)
                return null;
            else return res.Response.FirstOrDefault();
        }

        public async Task<List<UserAppPermission>> GetUserAppPermissionsByUserId(int? userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, dbType: DbType.Int64);
            var res = await ExecuteStoredProcedureListResult<UserAppPermission>(SprGetUserAppPermissions, parameters);
            if (res.Response.Count()<=0)
                return null;
            else return res.Response.AsList();
        }
    }
}