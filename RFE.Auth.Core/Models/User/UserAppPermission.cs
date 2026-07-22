using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RFE.Auth.Core.Models.App;

namespace RFE.Auth.Core.Models.User
{
    [Table("UserAppPermission", Schema ="AUTH")]
    public class UserAppPermission
    {
        [Key]
        public Guid UAPId { get; set; } 
        public Guid UserId { get; set; } 
        [ForeignKey("UserId")]
        public AuthUser AuthUser { get; set; }
        public Guid AppId { get; set; } 
        [ForeignKey("AppId")]
        public Application Application { get; set; }
        public Guid PermissionId { get; set; } 
        [ForeignKey("PermissionId")]
        public AppPermission AppPermission { get; set; }
    }
}