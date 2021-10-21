using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Repositories
{
    public interface IAuthUserRepository 
    {

        Task<List<AuthUser>> All();
        Task<AuthUser> GetById(int id);
        Task<bool> Add(UnconfirmedAppUser entity);
        Task<bool> Delete(int id);
        Task<bool> Upsert(AuthUser entity);
        Task<List<UnconfirmedAppUser>> GetUnConfirmedUsers();
        Task<UnconfirmedAppUser> GetUnConfirmedUsersById(int Id);
    }
}