using System.Threading.Tasks;
using RFE.Auth.Core.Models.Auth;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IJwtAuthenticationService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest authenticateRequest);
    }
}