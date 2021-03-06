using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<AuthUserByIdGetResponse>> GetAllRegisteredUsers();
        Task<AuthUserByIdGetResponse> GetUserById(int id);
        Task AddNewAuthUser(AuthUser authUser);
    }
}