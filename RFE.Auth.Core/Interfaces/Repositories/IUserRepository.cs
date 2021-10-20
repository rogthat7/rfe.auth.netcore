using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Repositories
{
    public interface IUserRepository 
    {

        Task<List<AppUser>> All();
        Task<AppUser> GetById(int id);
        Task<bool> Add(AppUser entity);
        Task<bool> Delete(int id);
        Task<bool> Upsert(AppUser entity);
        Task<List<AppUser>> GetUnConfirmedUsers();
        Task<AppUser> GetUnConfirmedUsersById(int Id);
    }
}