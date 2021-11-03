using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RFE.Auth.Core.Models.App
{
    [Index(nameof(PermissionName), IsUnique = true)]
    [Index(nameof(PermissionType), IsUnique = true)]
    [Table("AppPermission", Schema ="AUTH")]
    public class AppPermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PermissionId { get; set; }
        [Required]
        public string PermissionName { get; set; }
        [Required]
        public string PermissionType { get; set; }
    }
}
