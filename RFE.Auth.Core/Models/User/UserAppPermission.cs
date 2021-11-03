using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RFE.Auth.Core.Models.User
{
    [Table("UserAppPermission", Schema ="AUTH")]
    public class UserAppPermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UAPId { get; set; } 
        public int UserId { get; set; } 
        public int AppId { get; set; } 
        public int PermissionId { get; set; } 
    }
}