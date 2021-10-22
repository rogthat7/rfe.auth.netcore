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
        Task<AuthUserByIdGetResponse> GetById(int id);
        /// <summary>
        /// Adds new unconfirmed user to the Unconfirmed List
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> AddNewUnconfirmedUser(UnconfirmedAuthUser entity);
        Task<bool> DeleteById(int id);
        /// <summary>
        /// Deletes User with passed username from Unconfirmed List
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<bool> DeleteUnconfirmedUser(string username);
        Task<bool> Upsert(AuthUser entity);
        Task<List<UnconfirmedAuthUser>> GetUnConfirmedUsers();
        Task<UnconfirmedAuthUser> GetUnConfirmedUsersById(int Id);
    }
}