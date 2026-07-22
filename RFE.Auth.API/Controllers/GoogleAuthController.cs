using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Controllers
{
    /// <summary>
    /// Handles Google federated authentication challenge and callback endpoints.
    /// </summary>
    [ApiController]
    [Route("api/auth/google")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserService _userService;

        public GoogleAuthController(DatabaseContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// Challenges Google Federated Login.
        /// </summary>
        /// <param name="redirectUri">The local redirect URI to return to after authentication callback processing.</param>
        /// <returns>A ChallengeResult redirecting to Google's authentication portal.</returns>
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string redirectUri = "/", string role = "authUser")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback), new { redirectUri, role })
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Receives Google authentication details, logs the user in, registers them locally if needed, and issues a local sign-in cookie.
        /// </summary>
        /// <param name="redirectUri">The original local redirect URI to return to.</param>
        /// <returns>Redirects to the specified redirect URI, or returns Bad Request if federated authentication failed.</returns>
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string redirectUri = "/", string role = "authUser")
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "google_auth_failed", error_description = "Google authentication failed." });
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { error = "google_email_missing", error_description = "Email not returned by Google." });
            }

            var dbUser = await _context.AuthUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser == null)
            {
                var randomPass = Guid.NewGuid().ToString("N");
                var encodedPass = EncryptionHelper.EncodePasswordToBase64(randomPass);
                
                dbUser = new AuthUser
                {
                    Username = email,
                    Email = email,
                    Password = encodedPass,
                    IsVerified = true
                };

                await _userService.AddNewAuthUser(dbUser, role);
                
                dbUser = await _context.AuthUsers.FirstOrDefaultAsync(u => u.Email == email);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, dbUser.UserId?.ToString() ?? ""),
                new Claim(ClaimTypes.Name, dbUser.Username),
                new Claim(ClaimTypes.Email, dbUser.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Redirect(redirectUri);
        }
    }
}
