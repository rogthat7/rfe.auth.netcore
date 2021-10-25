
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
    public class AuthUserRepository: RepositoryBase, IAuthUserRepository
    {
        private const string SprGetUsers = "AUTH.spr_GetAllUsers";
        private const string SprGetUserById = "AUTH.spr_GetUserById";
        private const string SprGetUnconfirmedUserById = "AUTH.spr_GetUserById";
        private const string SprUpdateAuthUser = "AUTH.spr_UpdateAuthUser";
        private const string SprDeleteUserById = "AUTH.spr_DeleteUserById";
        private const string SprAuthenticataAuthUser = "AUTH.spr_AuthenticataAuthUser";
        private const string SprAddNewUser = "AUTH.spr_AddNewUser";

        public AuthUserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async Task AddNewConfirmedUser(AuthUser entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
           
            var parameters = new DynamicParameters();
            parameters.Add("@FirstName", entity.FirstName, dbType: DbType.String);
            parameters.Add("@LastName", entity.LastName, dbType: DbType.String);
            parameters.Add("@Email", entity.Email, dbType: DbType.String);
            parameters.Add("@Username", entity.Username, dbType: DbType.String);
            parameters.Add("@Password", entity.Password, dbType: DbType.String);
            parameters.Add("@Phone", entity.Phone, dbType: DbType.Int64);
            parameters.Add("@Confirmed", entity.Confirmed, dbType: DbType.Boolean);
            var res = await ExecuteStoredProcedureCreateResult(SprAddNewUser, parameters);
        }
        public async Task<List<AuthUserByIdGetResponse>> All()
        {
            var parameters = new DynamicParameters();
            var res = await ExecuteStoredProcedureListResult<AuthUserByIdGetResponse>(SprGetUsers, parameters);
            return res.Response as List<AuthUserByIdGetResponse>;
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

        public async Task<bool> DeleteById(int? id)
        {
            if (id == null)
                return false;
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);
            var res = await ExecuteStoredProcedureUpdateDeleteResult(SprDeleteUserById, parameters);
            if (res>0)
                return true;
            else
                return false;
        }

        public async Task<AuthUserByIdGetResponse> GetById(int? id)
        {
            if(id == null)
                return null;
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);
            var res = await ExecuteStoredProcedureListResult<AuthUserByIdGetResponse>(SprGetUserById, parameters);
            return res.Response.FirstOrDefault() as AuthUserByIdGetResponse;
        }

        public async Task<bool> Upsert(AuthUser entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
           
            var parameters = new DynamicParameters();
            parameters.Add("@FirstName", entity.FirstName, dbType: DbType.String);
            parameters.Add("@LastName", entity.LastName, dbType: DbType.String);
            parameters.Add("@Email", entity.Email, dbType: DbType.String);
            parameters.Add("@Username", entity.Username, dbType: DbType.String);
            parameters.Add("@Phone", entity.Phone, dbType: DbType.Int64);
            var res = await ExecuteStoredProcedureUpdateDeleteResult(SprUpdateAuthUser, parameters);
            if (res > 0)
                return true;
            else
                return false;
        }
    }
}