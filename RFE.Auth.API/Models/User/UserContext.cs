
using Microsoft.EntityFrameworkCore;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            
        }
        public virtual DbSet<AppUser> AppUsers {get; set;}
    }
}