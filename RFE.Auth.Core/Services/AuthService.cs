using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class AuthService : IAuthService
    {
        public Task<AuthenticateResponse> Authenticate(AuthenticateRequest authenticateRequestModel)
        {
            throw new System.NotImplementedException();
        }

    }
}