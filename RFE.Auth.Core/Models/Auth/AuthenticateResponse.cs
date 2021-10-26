using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Models.Auth
{
    public class AuthenticateResponse
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public Token Token { get; set; }


        public AuthenticateResponse(AuthUser user, Token token)
        {
            Id = user.UserId.Value;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            Token = token;
        }
    }
}