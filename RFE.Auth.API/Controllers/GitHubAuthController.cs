using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Controllers
{
    /// <summary>
    /// Handles GitHub federated authentication challenge and callback endpoints.
    /// </summary>
    [ApiController]
    [Route("api/auth/github")]
    public class GitHubAuthController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<GitHubAuthController> _logger;

        public GitHubAuthController(DatabaseContext context, IUserService userService, ILogger<GitHubAuthController> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Challenges GitHub Federated Login.
        /// </summary>
        /// <param name="redirectUri">The local redirect URI to return to after authentication callback processing.</param>
        /// <returns>A ChallengeResult redirecting to GitHub's authentication portal.</returns>
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string redirectUri = "/", string role = "authUser")
        {
            if (HttpContext.Request.Path.Value.Contains("favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("GitHub Login challenged. RequestId: {RequestId}, redirectUri: {RedirectUri}, Role: {Role}", requestId, redirectUri, role);

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback), new { redirectUri, role })
            };
            return Challenge(properties, "GitHub");
        }

        /// <summary>
        /// Receives GitHub authentication details, logs the user in, registers them locally if needed, and issues a local sign-in cookie.
        /// </summary>
        /// <param name="redirectUri">The original local redirect URI to return to.</param>
        /// <returns>Redirects to the specified redirect URI, or returns Bad Request if federated authentication failed.</returns>
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string redirectUri = "/", string role = "appUser")
        {
            if (HttpContext.Request.Path.Value.Contains("favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("GitHub Callback started. RequestId: {RequestId}, Role: {Role}", requestId, role);

            var result = await HttpContext.AuthenticateAsync("GitHub");
            if (!result.Succeeded)
            {
                _logger.LogWarning("GitHub authentication failed. RequestId: {RequestId}", requestId);
                return BadRequest(new { error = "github_auth_failed", error_description = "GitHub authentication failed." });
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email not returned by GitHub. RequestId: {RequestId}", requestId);
                return BadRequest(new { error = "github_email_missing", error_description = "Email not returned by GitHub." });
            }

            _logger.LogInformation("Retrieving user/verifier from DB context for email: {Email}, RequestId: {RequestId}", email, requestId);
            var dbUser = await _context.AuthUsers.FirstOrDefaultAsync(u => u.Email == email);
            
            if (dbUser == null)
            {
                _logger.LogInformation("User not found. Registering new local user for email: {Email}, Role: {Role}, RequestId: {RequestId}", email, role, requestId);
                
                var randomPass = Guid.NewGuid().ToString("N");
                var encodedPass = EncryptionHelper.EncodePasswordToBase64(randomPass);

                var usernameClaim = result.Principal.FindFirst("urn:github:name")?.Value ?? result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? email;
                
                dbUser = new AuthUser
                {
                    Username = usernameClaim,
                    Email = email,
                    Password = encodedPass,
                    IsVerified = true
                };

                await _userService.AddNewAuthUser(dbUser, role);
                await _userService.MarkUserAsVerified(dbUser.Username);
                
                dbUser = await _context.AuthUsers.FirstOrDefaultAsync(u => u.Email == email);
            }

            _logger.LogInformation("Signing in user in local cookie scheme: {Username}, RequestId: {RequestId}", dbUser.Username, requestId);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, dbUser.UserId?.ToString() ?? ""),
                new Claim(ClaimTypes.Name, dbUser.Username),
                new Claim(ClaimTypes.Email, dbUser.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            _logger.LogInformation("GitHub auth successful. Redirecting to: {RedirectUri}, RequestId: {RequestId}", redirectUri, requestId);
            return Redirect(redirectUri);
        }
    }
}
