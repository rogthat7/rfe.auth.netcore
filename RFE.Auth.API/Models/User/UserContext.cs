
using Microsoft.EntityFrameworkCore;
using RFE.Auth.Core.Models.App;
using RFE.Auth.Core.Models.Role;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            
        }
        public virtual DbSet<AuthUser> AuthUsers {get; set;}
        public virtual DbSet<AppPermission> AppPermissions {get; set;}
        public virtual DbSet<Roles> Roles {get; set;}
        public virtual DbSet<UserRole> UserRoles {get; set;}
        public virtual DbSet<Application> Apps {get; set;}
        public virtual DbSet<UserAppPermission> UserAppPermissions {get; set;}
    }
}