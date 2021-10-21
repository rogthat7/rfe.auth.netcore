
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
    public class UserRepository: RepositoryBase, IUserRepository
    {
        private const string SprGetUsers = "AUTH.spr_GetAllUsers";
        private const string SprGetUserById = "AUTH.spr_GetUserById";
        private const string SprGetUnconfirmedUserById = "AUTH.spr_GetUnconfirmedUserById";
        private const string SprGetUnconfirmedUsers = "AUTH.spr_GetUnconfirmedUsers";

        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public  Task<bool> Add(AppUser entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AppUser>> All()
        {
            var parameters = new DynamicParameters();
            var res = await ExecuteStoredProcedureListResult<AppUser>(SprGetUsers, parameters);
            return res.Response as List<AppUser>;
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<AppUser> GetById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", DbType.Int32);
            var res = await ExecuteStoredProcedureListResult<dynamic>(SprGetUserById, parameters);
            return res.Response.FirstOrDefault() as AppUser;
        }

        public async Task<List<AppUser>> GetUnConfirmedUsers()
        {
            var parameters = new DynamicParameters();
            var res = await ExecuteStoredProcedureListResult<AppUser>(SprGetUnconfirmedUsers, parameters);
            return res.Response as List<AppUser>;
        }
        public async Task<AppUser> GetUnConfirmedUsersById(int Id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", DbType.Int32);
            var res = await ExecuteStoredProcedureListResult<AppUser>(SprGetUnconfirmedUserById, parameters);
            return res.Response.FirstOrDefault() as AppUser;
        }
        public async Task<bool> Upsert(AppUser entity)
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

            return true;
        }
    }
}