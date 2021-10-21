using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
    [Serializable]
    public class AuthUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public long Phone { get; set; }
        public virtual bool Confirmed { get; set; } = false;

        [JsonIgnore]
        public string Password { get; set; }
    }
}