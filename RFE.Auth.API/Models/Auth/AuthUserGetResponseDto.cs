using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    public class AuthUserAuthenticateResponseDto : AuthenticateResponse
    {
        public AuthUserAuthenticateResponseDto(AuthUser user, Token token) : base(user, token)
        {
        }
    }
}