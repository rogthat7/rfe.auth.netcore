using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Models.Auth
{
    public class AuthenticateResponse
    {
        public int? Id { get; set; }
        public string Username { get; set; }
        public Token Token { get; set; }


        public AuthenticateResponse(AuthUser user, Token token)
        {
            Id = user.UserId.Value;
            Username = user.Username;
            Token = token;
        }
    }
}