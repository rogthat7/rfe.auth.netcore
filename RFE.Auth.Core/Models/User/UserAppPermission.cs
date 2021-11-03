using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RFE.Auth.Core.Models.App;

namespace RFE.Auth.Core.Models.User
{
    [Table("UserAppPermission", Schema ="AUTH")]
    public class UserAppPermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UAPId { get; set; } 
        public int UserId { get; set; } 
        [ForeignKey("UserId")]
        public AuthUser AuthUser { get; set; }
        public int AppId { get; set; } 
        [ForeignKey("AppId")]
        public Application Application { get; set; }
        public int PermissionId { get; set; } 
        [ForeignKey("PermissionId")]
        public AppPermission AppPermission { get; set; }
    }
}