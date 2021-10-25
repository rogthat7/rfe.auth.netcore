using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    public class AuthUserGetResponseDto : AuthenticateResponse
    {
        public AuthUserGetResponseDto(AuthUser user, Token token) : base(user, token)
        {
        }
    }
}