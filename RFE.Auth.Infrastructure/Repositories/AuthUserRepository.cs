
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
        private const string SprGetUnconfirmedUserById = "AUTH.spr_GetUnconfirmedUserById";
        private const string SprUpdateConfirmedUser = "AUTH.spr_UpdateConfirmedUser";
        private const string SprDeleteUserById = "AUTH.spr_SprDeleteUserById";
        private const string SprGetUnconfirmedUsers = "AUTH.spr_GetUnconfirmedUsers";
        private const string SprAddNewUser = "AUTH.spr_SprAddNewUser";

        public AuthUserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async  Task<bool> Add(UnconfirmedAppUser entity)
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
            var res = await ExecuteStoredProcedureCreateResult(SprAddNewUser, parameters);
            if (res > 0)
                return true;
            else
                return false;
        }

        public async Task<List<AuthUser>> All()
        {
            var parameters = new DynamicParameters();
            var res = await ExecuteStoredProcedureListResult<AuthUser>(SprGetUsers, parameters);
            return res.Response as List<AuthUser>;
        }

        public async Task<bool> Delete(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);
            var res = await ExecuteStoredProcedureUpdateDeleteResult(SprDeleteUserById, parameters);
            if (res>0)
                return true;
            else
                return false;
        }

        public async Task<AuthUser> GetById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);
            var res = await ExecuteStoredProcedureListResult<AuthUser>(SprGetUserById, parameters);
            return res.Response.FirstOrDefault() as AuthUser;
        }

        public async Task<List<UnconfirmedAppUser>> GetUnConfirmedUsers()
        {
            var parameters = new DynamicParameters();
            var res = await ExecuteStoredProcedureListResult<UnconfirmedAppUser>(SprGetUnconfirmedUsers, parameters);
            return res.Response as List<UnconfirmedAppUser>;
        }
        public async Task<UnconfirmedAppUser> GetUnConfirmedUsersById(int Id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", DbType.Int32);
            var res = await ExecuteStoredProcedureListResult<UnconfirmedAppUser>(SprGetUnconfirmedUserById, parameters);
            return res.Response.FirstOrDefault() as UnconfirmedAppUser;
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