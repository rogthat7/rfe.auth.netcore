
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
        private const string SprUpdateConfirmedUser = "AUTH.spr_UpdateConfirmedUser";
        private const string SprDeleteUserById = "AUTH.spr_SprDeleteUserById";
        private const string SprGetUnconfirmedUsers = "AUTH.spr_GetUnconfirmedUsers";
        private const string SprAddNewUser = "AUTH.spr_SprAddNewUser";
        private readonly string SprDeleteUnconfirmedUserBy = "AUTH.spr_SprDeleteUnconfirmedUser";

        public AuthUserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async  Task<bool> AddNewUnconfirmedUser(UnconfirmedAuthUser entity)
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
            var res = await ExecuteStoredProcedureCreateResult(SprAddNewUser, parameters);
            if (res >= 0)
                return true;
            else
                return false;
        }
        public async Task<List<AuthUserByIdGetResponse>> All()
        {
            var parameters = new DynamicParameters();
            var res = await ExecuteStoredProcedureListResult<AuthUserByIdGetResponse>(SprGetUsers, parameters);
            return res.Response as List<AuthUserByIdGetResponse>;
        }

        public async Task<bool> DeleteById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);
            var res = await ExecuteStoredProcedureUpdateDeleteResult(SprDeleteUserById, parameters);
            if (res>0)
                return true;
            else
                return false;
        }

        public async Task<bool> DeleteUnconfirmedUser(string username)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", username, DbType.String);
            var res = await ExecuteStoredProcedureUpdateDeleteResult(SprDeleteUnconfirmedUserBy, parameters);
            if (res>0)
                return true;
            else
                return false;
        }

        public async Task<AuthUserByIdGetResponse> GetById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);
            var res = await ExecuteStoredProcedureListResult<AuthUserByIdGetResponse>(SprGetUserById, parameters);
            return res.Response.FirstOrDefault() as AuthUserByIdGetResponse;
        }

        public async Task<List<UnconfirmedAuthUser>> GetUnConfirmedUsers()
        {
            var parameters = new DynamicParameters();
            var res = await ExecuteStoredProcedureListResult<UnconfirmedAuthUser>(SprGetUnconfirmedUsers, parameters);
            return res.Response as List<UnconfirmedAuthUser>;
        }

        public async Task<UnconfirmedAuthUser> GetUnConfirmedUsersById(int Id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", DbType.Int32);
            var res = await ExecuteStoredProcedureListResult<UnconfirmedAuthUser>(SprGetUnconfirmedUserById, parameters);
            return res.Response.FirstOrDefault() as UnconfirmedAuthUser;
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
            var res = await ExecuteStoredProcedureUpdateDeleteResult(SprUpdateConfirmedUser, parameters);
            if (res > 0)
                return true;
            else
                return false;
        }
    }
}