using System.ComponentModel.DataAnnotations;

namespace RFE.Auth.API.Models.User
{
    public class SendPhoneDto 
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public long Phone { get; set; }
        
        [Required]
        public string Password { get; set; }

        public string? Role { get; set; }
    }
}
