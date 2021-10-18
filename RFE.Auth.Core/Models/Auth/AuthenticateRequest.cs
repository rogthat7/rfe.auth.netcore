
using System.ComponentModel.DataAnnotations;

namespace RFE.Auth.Core.Models.Auth
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}