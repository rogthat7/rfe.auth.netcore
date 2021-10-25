using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Repositories
{
    /// <summary>
    /// IAuthUserRepository
    /// </summary>
    public interface IAuthUserRepository 
    {

        Task<List<AuthUserByIdGetResponse>> All();
        Task<AuthUserByIdGetResponse> GetById(int? id);
        /// <summary>
        /// Adds new unconfirmed user to the Unconfirmed List
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddNewConfirmedUser(AuthUser entity);
        Task<bool> DeleteById(int? id);
        Task<bool> Upsert(AuthUser entity);
        Task<AuthUser> AuthenticateAuthUser(string username, string password);
    }
}