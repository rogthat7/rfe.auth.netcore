using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    public class AuthUserGetResponseDto : AuthenticateResponse
    {
        public AuthUserGetResponseDto(AppUser user, string token) : base(user, token)
        {
        }
    }
}