using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
     public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Phone { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
    }
}