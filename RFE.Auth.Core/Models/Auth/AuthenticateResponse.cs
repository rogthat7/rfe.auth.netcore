using System;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Models.Auth
{
    public class AuthenticateResponse
    {
        public Guid?    Id         { get; set; }
        public string   Username   { get; set; }
        public bool     IsVerified { get; set; }
        public Token    Token      { get; set; }
        public AuthUser User       { get; set; }


        public AuthenticateResponse(AuthUser user, Token token)
        {
            Id         = user.UserId;
            Username   = user.Username;
            IsVerified = user.IsVerified;
            Token      = token;
            User       = user;
        }
    }
}