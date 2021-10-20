
using System;
using System.Collections.Generic;
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

        public Task<AppUser> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<AppUser>> GetUnConfirmedUsers()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Upsert(AppUser entity)
        {
            throw new NotImplementedException();
        }
    }
}