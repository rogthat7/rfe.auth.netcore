using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
    [Serializable]
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Phone), IsUnique = true)]
    [Table("AuthUser", Schema ="AUTH")]
    public class AuthUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int?     UserId      { get; set; }
        [Required]
        public string   Username    { get; set; }
        public string   Email       { get; set; }
        public long?    Phone       { get; set; }
        [Required]
        public virtual string Password { get; set; }
        public bool     IsVerified  { get; set; } = false;
    }
}