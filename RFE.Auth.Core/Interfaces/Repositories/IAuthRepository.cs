using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Repositories
{
    public interface IAuthRepository 
    {
        
        Task<AuthUser> AuthenticateAuthUser(string username, string password);
        Task<List<UserAppPermission>> GetUserAppPermissionsByUserId(int? userId);
    }

}