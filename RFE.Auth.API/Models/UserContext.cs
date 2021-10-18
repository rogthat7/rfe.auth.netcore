
using Microsoft.EntityFrameworkCore;
using RFE.Auth.Core.Models.Users;

namespace RFE.Auth.API.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            
        }
        public DbSet<AppUser> AppUsers {get; set;}
    }
}