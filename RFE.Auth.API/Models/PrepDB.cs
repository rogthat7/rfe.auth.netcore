using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Models.App;
using RFE.Auth.Core.Models.Role;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models
{
    public static class PrepDB
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<DatabaseContext>());
            }
        }

        private static void SeedData(DatabaseContext databaseContext)
        {
            System.Console.WriteLine("Applying Migrations");
            databaseContext.Database.Migrate();

            if(!(databaseContext.AuthUsers.Any() 
                && databaseContext.Apps.Any() 
                && databaseContext.Roles.Any() 
                && databaseContext.UserRoles.Any()
                && databaseContext.AppPermissions.Any()
                && databaseContext.UserAppPermissions.Any() 
                ))
            {
                System.Console.WriteLine("Adding Data - Seeding...");
                databaseContext.AuthUsers.AddRange(
                    new AuthUser() {
                        Email = "admin@fish-tracker.com",
                        Username = "admin@fish-tracker.com",
                        Password = EncryptionHelper.EncodePasswordToBase64("admin"),
                        Phone = 8806329362
                    }
                );
                databaseContext.Apps.AddRange(
                    new Application() {
                        AppName = "fish-tracker"
                    }
                );
                databaseContext.Roles.AddRange(
                    new Roles() {
                        RoleName = "appUser"
                    },
                    new Roles() {
                        RoleName = "authUser"
                    }
                );
                databaseContext.UserRoles.AddRange(
                    new UserRole() {
                        AppId = 1,
                        RoleId = 1,
                        UserId = 1
                    }
                );
                databaseContext.AppPermissions.AddRange(
                    new AppPermission() {
                        PermissionName = "modbase",
                        PermissionType = "BASIC"
                    }
                );
                databaseContext.UserAppPermissions.AddRange(
                    new UserAppPermission() {
                        UserId = 1,
                        AppId = 1,
                        PermissionId = 1
                    }
                );
                databaseContext.SaveChanges();
            }
            else
            {
                System.Console.WriteLine("Already Has Data - Not Seeding");                
            }
        }

        private static string HashPassword(string password)
        {
            // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }
            Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}