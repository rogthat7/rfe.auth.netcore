using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RFE.Auth.Core.Models.Role;

namespace RFE.Auth.Core.Models.App
{
    [Table("AppRole", Schema = "AUTH")]
    public class AppRole
    {
        [Key]
        public Guid AppRoleId { get; set; }

        [Required]
        public Guid AppId { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey("AppId")]
        public Application Application { get; set; }

        [ForeignKey("RoleId")]
        public Roles Role { get; set; }
    }
}
