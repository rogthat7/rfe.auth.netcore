using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RFE.Auth.Core.Models.App
{
    [Index(nameof(AppName), IsUnique = true)]
    [Table("Application", Schema ="AUTH")]
    public class Application
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppId { get; set; }
        [Required]
        public string AppName { get; set; }
    }
}