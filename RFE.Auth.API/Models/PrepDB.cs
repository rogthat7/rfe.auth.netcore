using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models
{
    public static class PrepDB
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<UserContext>());
            }
        }

        private static void SeedData(UserContext userContext)
        {
            System.Console.WriteLine("Applying Migrations");
            userContext.Database.Migrate();

            if(!userContext.AppUsers.Any())
            {
                System.Console.WriteLine("Adding Data - Seeding...");
                userContext.AppUsers.AddRange(
                    new AppUser() {
                        Email = "admin@fish-tracker.com",
                        FirstName = "Admin",
                        Username = "admin@fish-tracker.com",
                        LastName = "Fish-Tracker",
                        Password = HashPassword("admin"),
                        Confirmed = true,
                        Phone = 8806329362
                    }
                );
                userContext.SaveChanges();
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