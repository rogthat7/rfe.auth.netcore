
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

        public async Task AddNewAuthUser(AuthUser entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
           
            const string sql = @"INSERT INTO ""AUTH"".""AuthUser"" (""Email"", ""Username"", ""Password"", ""Phone"") VALUES (@Email, @Username, @Password, @Phone)";
            await _unitOfWork.DbConnection.ExecuteAsync(sql, new { entity.Email, entity.Username, entity.Password, entity.Phone });
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