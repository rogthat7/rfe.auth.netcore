using System;
using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
    [Serializable]
    public class UnconfirmedAuthUser 
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public long Phone { get; set; }
        public virtual string Password { get; set; }
    }
}