using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    /// <summary>
    /// AuthUserAddPostRequestDto
    /// </summary>
    public class AuthUserAddPostRequestDto 
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public long? Phone { get; set; }
        public virtual string Password { get; set; }
    }
}