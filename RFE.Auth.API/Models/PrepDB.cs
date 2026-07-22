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
                SeedOpenIddictClientsAsync(serviceScope.ServiceProvider).GetAwaiter().GetResult();
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
            
            if (roleAppUser.RoleId == Guid.Empty) databaseContext.Roles.Add(roleAppUser);
            if (roleAuthUser.RoleId == Guid.Empty) databaseContext.Roles.Add(roleAuthUser);
            if (roleLaborer.RoleId == Guid.Empty) databaseContext.Roles.Add(roleLaborer);
            if (roleEmployer.RoleId == Guid.Empty) databaseContext.Roles.Add(roleEmployer);
            if (rolePanchayat.RoleId == Guid.Empty) databaseContext.Roles.Add(rolePanchayat);
            if (roleAdmin.RoleId == Guid.Empty) databaseContext.Roles.Add(roleAdmin);
            if (roleLabourer.RoleId == Guid.Empty) databaseContext.Roles.Add(roleLabourer);
            if (roleJobCreator.RoleId == Guid.Empty) databaseContext.Roles.Add(roleJobCreator);
            databaseContext.SaveChanges();

            // 2. Ensure Applications
            var appFish = databaseContext.Apps.FirstOrDefault(a => a.AppName == "fish-tracker") ?? new Application { AppName = "fish-tracker", DisplayName = "Fish Tracker" };
            var appAuth = databaseContext.Apps.FirstOrDefault(a => a.AppName == "rfe-auth") ?? new Application { AppName = "rfe-auth", DisplayName = "RFE Auth" };
            var appGlam = databaseContext.Apps.FirstOrDefault(a => a.AppName == "rfe-glam-app") ?? new Application { AppName = "rfe-glam-app", DisplayName = "RFE Glam App" };
            if (appFish.AppId == Guid.Empty) databaseContext.Apps.Add(appFish);
            if (appAuth.AppId == Guid.Empty) databaseContext.Apps.Add(appAuth);
            if (appGlam.AppId == Guid.Empty) databaseContext.Apps.Add(appGlam);
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
                (userEmployer.UserId, roleJobCreator.RoleId, appGlam.AppId),
                (userLaborer.UserId, roleLabourer.RoleId, appGlam.AppId),
                (userPanchayat.UserId, rolePanchayat.RoleId, appGlam.AppId),
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
            if (permBase.PermissionId == Guid.Empty) databaseContext.AppPermissions.Add(permBase);
            databaseContext.SaveChanges();

            // 6. Ensure UserAppPermission
            var appPerms = new[] {
                (userSystemAdmin.UserId, appAuth.AppId),
                (userFishAdmin.UserId, appFish.AppId),
                (userEmployer.UserId, appGlam.AppId),
                (userLaborer.UserId, appGlam.AppId),
                (userPanchayat.UserId, appGlam.AppId),
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
            RandomNumberGenerator.Fill(salt);
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

        private static async System.Threading.Tasks.Task SeedOpenIddictClientsAsync(IServiceProvider serviceProvider)
        {
            var manager = serviceProvider.GetRequiredService<OpenIddict.Abstractions.IOpenIddictApplicationManager>();

            var app = await manager.FindByClientIdAsync("rfe-auth-app");
            // Also clean up old client ID if it still exists
            var oldApp = await manager.FindByClientIdAsync("mock-external-app");
            if (oldApp != null) { await manager.DeleteAsync(oldApp); }

            var descriptor = new OpenIddict.Abstractions.OpenIddictApplicationDescriptor
            {
                ClientId = "rfe-auth-app",
                DisplayName = "RFE Auth App",
                ClientType = OpenIddict.Abstractions.OpenIddictConstants.ClientTypes.Public,
                Permissions =
                {
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                },
                RedirectUris =
                {
                    new Uri("https://oauth.pstmn.io/v1/callback"),
                    new Uri("http://localhost:3001/oauth-callback"),
                    new Uri("https://localhost:3001/oauth-callback"),
                    new Uri("https://localhost:5173/oauth-callback")
                }
            };

            if (app == null)
            {
                await manager.CreateAsync(descriptor);
            }
            else
            {
                await manager.UpdateAsync(app, descriptor);
            }

            // Seed rfe-glam-app (third-party client)
            var glamApp = await manager.FindByClientIdAsync("rfe-glam-app");
            var glamDescriptor = new OpenIddict.Abstractions.OpenIddictApplicationDescriptor
            {
                ClientId = "rfe-glam-app",
                DisplayName = "RFE Glam App",
                ClientType = OpenIddict.Abstractions.OpenIddictConstants.ClientTypes.Public,
                Permissions =
                {
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddict.Abstractions.OpenIddictConstants.Permissions.Prefixes.Scope + "openid"
                },
                RedirectUris =
                {
                    new Uri("http://localhost:3000/oauth-callback"),
                    new Uri("http://localhost:5173/oauth-callback"),
                    new Uri("https://localhost:5173/oauth-callback"),
                    new Uri("http://localhost:6000/oauth-callback"),
                    new Uri("https://localhost:6000/oauth-callback")
                }
            };

            if (glamApp == null)
            {
                await manager.CreateAsync(glamDescriptor);
            }
            else
            {
                await manager.UpdateAsync(glamApp, glamDescriptor);
            }
        }
    }
}