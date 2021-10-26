using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthUser> AuthenticateAuthUser(AuthenticateRequest authenticateRequestModel);
        Task<List<UserAppPermission>> GetUserAppPermissions(int? UserId);
    }
}