using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RFE.Auth.Core.Models.User
{
    [Table("UserRole", Schema ="AUTH")]
    public class UserRole
    {
        [Key]
        public Guid UserRoleId { get; set; }
        [ForeignKey("UserId")]
        public AuthUser AuthUser { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid AppId { get; set; }
    }
}
