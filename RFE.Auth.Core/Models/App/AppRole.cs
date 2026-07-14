using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RFE.Auth.Core.Models.Role;

namespace RFE.Auth.Core.Models.App
{
    [Table("AppRole", Schema = "AUTH")]
    public class AppRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppRoleId { get; set; }

        [Required]
        public int AppId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("AppId")]
        public Application Application { get; set; }

        [ForeignKey("RoleId")]
        public Roles Role { get; set; }
    }
}
