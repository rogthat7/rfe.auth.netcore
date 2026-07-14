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
            var roleLabourer = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "Labourer") ?? new Roles { RoleName = "Labourer" };
            var roleJobCreator = databaseContext.Roles.FirstOrDefault(r => r.RoleName == "JobCreator") ?? new Roles { RoleName = "JobCreator" };
            
            if (roleAppUser.RoleId == 0) databaseContext.Roles.Add(roleAppUser);
            if (roleAuthUser.RoleId == 0) databaseContext.Roles.Add(roleAuthUser);
            if (roleLaborer.RoleId == 0) databaseContext.Roles.Add(roleLaborer);
            if (roleEmployer.RoleId == 0) databaseContext.Roles.Add(roleEmployer);
            if (rolePanchayat.RoleId == 0) databaseContext.Roles.Add(rolePanchayat);
            if (roleAdmin.RoleId == 0) databaseContext.Roles.Add(roleAdmin);
            if (roleLabourer.RoleId == 0) databaseContext.Roles.Add(roleLabourer);
            if (roleJobCreator.RoleId == 0) databaseContext.Roles.Add(roleJobCreator);
            databaseContext.SaveChanges();

            // 2. Ensure Applications
            var appFish = databaseContext.Apps.FirstOrDefault(a => a.AppName == "fish-tracker") ?? new Application { AppName = "fish-tracker" };
            var appAuth = databaseContext.Apps.FirstOrDefault(a => a.AppName == "rfe-auth") ?? new Application { AppName = "rfe-auth" };
            if (appFish.AppId == 0) databaseContext.Apps.Add(appFish);
            if (appAuth.AppId == 0) databaseContext.Apps.Add(appAuth);
            databaseContext.SaveChanges();

            // 3. Ensure AuthUsers
            var userSystemAdmin = databaseContext.AuthUsers.FirstOrDefault(u => u.Username == "admin") ?? new AuthUser {
                Email = "admin@rfeauth.com",
                Username = "admin",
                Password = EncryptionHelper.EncodePasswordToBase64("rogthat7"),
                Phone = 9999900000,
                IsVerified = true
            };
            var userFishAdmin = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 8806329362) ?? new AuthUser {
                Email = "admin@fish-tracker.com",
                Username = "admin@fish-tracker.com",
                Password = EncryptionHelper.EncodePasswordToBase64("admin"),
                Phone = 8806329362,
                IsVerified = true
            };
            var userEmployer = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543210) ?? new AuthUser {
                Email = "employer@auth.com",
                Username = "9876543210",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543210,
                IsVerified = true
            };
            var userLaborer = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543211) ?? new AuthUser {
                Email = "laborer@auth.com",
                Username = "9876543211",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543211,
                IsVerified = true
            };
            var userPanchayat = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543212) ?? new AuthUser {
                Email = "panchayat@auth.com",
                Username = "9876543212",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543212,
                IsVerified = true
            };
            var userAdmin = databaseContext.AuthUsers.FirstOrDefault(u => u.Phone == 9876543213) ?? new AuthUser {
                Email = "admin@auth.com",
                Username = "9876543213",
                Password = EncryptionHelper.EncodePasswordToBase64("hashed"),
                Phone = 9876543213,
                IsVerified = true
            };

            userSystemAdmin.IsVerified = true;
            userFishAdmin.IsVerified = true;
            userEmployer.IsVerified = true;
            userLaborer.IsVerified = true;
            userPanchayat.IsVerified = true;
            userAdmin.IsVerified = true;

            if (userSystemAdmin.UserId == null) databaseContext.AuthUsers.Add(userSystemAdmin);
            if (userFishAdmin.UserId == null) databaseContext.AuthUsers.Add(userFishAdmin);
            if (userEmployer.UserId == null) databaseContext.AuthUsers.Add(userEmployer);
            if (userLaborer.UserId == null) databaseContext.AuthUsers.Add(userLaborer);
            if (userPanchayat.UserId == null) databaseContext.AuthUsers.Add(userPanchayat);
            if (userAdmin.UserId == null) databaseContext.AuthUsers.Add(userAdmin);
            databaseContext.SaveChanges();

            // 4. Ensure UserRoles
            var mappings = new[] {
                (userSystemAdmin.UserId, roleAdmin.RoleId, appAuth.AppId),
                (userFishAdmin.UserId, roleAppUser.RoleId, appFish.AppId),
                (userEmployer.UserId, roleEmployer.RoleId, appAuth.AppId),
                (userLaborer.UserId, roleLaborer.RoleId, appAuth.AppId),
                (userPanchayat.UserId, rolePanchayat.RoleId, appAuth.AppId),
                (userAdmin.UserId, roleAdmin.RoleId, appAuth.AppId),
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
                (userSystemAdmin.UserId, appAuth.AppId),
                (userFishAdmin.UserId, appFish.AppId),
                (userEmployer.UserId, appAuth.AppId),
                (userLaborer.UserId, appAuth.AppId),
                (userPanchayat.UserId, appAuth.AppId),
                (userAdmin.UserId, appAuth.AppId),
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