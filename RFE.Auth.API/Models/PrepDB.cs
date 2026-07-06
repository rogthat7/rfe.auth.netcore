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

            System.Console.WriteLine("Adding Data - Seeding...");

            // 1. Ensure Roles
            var roleAppUser = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "appUser") ?? new Roles { RoleName = "appUser" };
            var roleAuthUser = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "authUser") ?? new Roles { RoleName = "authUser" };
            var roleLaborer = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "Laborer") ?? new Roles { RoleName = "Laborer" };
            var roleEmployer = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "Employer") ?? new Roles { RoleName = "Employer" };
            var rolePanchayat = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "PanchayatAdmin") ?? new Roles { RoleName = "PanchayatAdmin" };
            var roleAdmin = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "Admin") ?? new Roles { RoleName = "Admin" };
            
            if (roleAppUser.RoleId == 0) databaseContext.Roles.Add(roleAppUser);
            if (roleAuthUser.RoleId == 0) databaseContext.Roles.Add(roleAuthUser);
            if (roleLaborer.RoleId == 0) databaseContext.Roles.Add(roleLaborer);
            if (roleEmployer.RoleId == 0) databaseContext.Roles.Add(roleEmployer);
            if (rolePanchayat.RoleId == 0) databaseContext.Roles.Add(rolePanchayat);
            if (roleAdmin.RoleId == 0) databaseContext.Roles.Add(roleAdmin);
            databaseContext.SaveChanges();

            // 2. Ensure Applications
            var appFish = databaseContext.Apps.FirstOrDefault(a => a.AppName == "fish-tracker") ?? new Application { AppName = "fish-tracker" };
            var appGlam = databaseContext.Apps.FirstOrDefault(a => a.AppName == "rfe-glam") ?? new Application { AppName = "rfe-glam" };
            if (appFish.AppId == 0) databaseContext.Apps.Add(appFish);
            if (appGlam.AppId == 0) databaseContext.Apps.Add(appGlam);
            databaseContext.SaveChanges();

            // 3. Ensure AuthUsers
            var userFishAdmin = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 8806329362) ?? new AuthUser {
                Email = "admin@fish-tracker.com",
                Username = "admin@fish-tracker.com",
                Password = EncryptionHelper.EncodePasswordToBase64("admin"),
                Phone = 8806329362
            };
            var userEmployer = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543210) ?? new AuthUser {
                Email = "employer@glam.com",
                Username = "9876543210",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543210
            };
            var userLaborer = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543211) ?? new AuthUser {
                Email = "laborer@glam.com",
                Username = "9876543211",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543211
            };
            var userPanchayat = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543212) ?? new AuthUser {
                Email = "panchayat@glam.com",
                Username = "9876543212",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543212
            };
            var userAdmin = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543213) ?? new AuthUser {
                Email = "admin@glam.com",
                Username = "9876543213",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543213
            };

            if (userFishAdmin.UserId == null) databaseContext.AuthUsers.Add(userFishAdmin);
            if (userEmployer.UserId == null) databaseContext.AuthUsers.Add(userEmployer);
            if (userLaborer.UserId == null) databaseContext.AuthUsers.Add(userLaborer);
            if (userPanchayat.UserId == null) databaseContext.AuthUsers.Add(userPanchayat);
            if (userAdmin.UserId == null) databaseContext.AuthUsers.Add(userAdmin);
            databaseContext.SaveChanges();

            // 4. Ensure UserRoles
            var mappings = new[] {
                (userFishAdmin.UserId, roleAppUser.RoleId, appFish.AppId),
                (userEmployer.UserId, roleEmployer.RoleId, appGlam.AppId),
                (userLaborer.UserId, roleLaborer.RoleId, appGlam.AppId),
                (userPanchayat.UserId, rolePanchayat.RoleId, appGlam.AppId),
                (userAdmin.UserId, roleAdmin.RoleId, appGlam.AppId),
            };

            foreach (var (uId, rId, aId) in mappings)
            {
                if (uId != null && !databaseContext.UserRoles.Any(ur => ur.UserId == uId.Value && ur.RoleId == rId && ur.AppId == aId))
                {
                    databaseContext.UserRoles.Add(new UserRole { UserId = uId.Value, RoleId = rId, AppId = aId });
                }
            }
            databaseContext.SaveChanges();

            // 5. Ensure AppPermission
            var permBase = databaseContext.AppPermissions.FirstOrDefault(p => p.PermissionName == "modbase") ?? new AppPermission {
                PermissionName = "modbase",
                PermissionType = "BASIC"
            };
            if (permBase.PermissionId == 0) databaseContext.AppPermissions.Add(permBase);
            databaseContext.SaveChanges();

            // 6. Ensure UserAppPermission
            var appPerms = new[] {
                (userFishAdmin.UserId, appFish.AppId),
                (userEmployer.UserId, appGlam.AppId),
                (userLaborer.UserId, appGlam.AppId),
                (userPanchayat.UserId, appGlam.AppId),
                (userAdmin.UserId, appGlam.AppId),
            };

            foreach (var (uId, aId) in appPerms)
            {
                if (uId != null && !databaseContext.UserAppPermissions.Any(uap => uap.UserId == uId.Value && uap.AppId == aId && uap.PermissionId == permBase.PermissionId))
                {
                    databaseContext.UserAppPermissions.Add(new UserAppPermission { UserId = uId.Value, AppId = aId, PermissionId = permBase.PermissionId });
                }
            }
            databaseContext.SaveChanges();
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