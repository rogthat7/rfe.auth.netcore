using System.ComponentModel.DataAnnotations;

namespace RFE.Auth.API.Models.User
{
    public class ConfirmPhoneRequest
    {
        [Required]
        public string TokenPayload { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
