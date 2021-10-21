using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IAuthUserService
    {
        Task<List<AuthUser>> GetAllRegisteredUsers();
        
        Task<AuthUser> GetUserById(int id);
        Task AddNewAuthUser(UnconfirmedAppUser authUser);
    }
}