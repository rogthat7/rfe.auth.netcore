using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using RFE.Auth.API.Models.User;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RFE.Auth.Core.Interfaces.Services;

namespace RFE.Auth.API.Controllers
{
    /// <summary>
    /// Implements OAuth 2.1 / OpenID Connect authorization server endpoints using OpenIddict.
    /// Supports Authorization Code Flow, PKCE, Refresh Tokens, and User Consent.
    /// </summary>
    [ApiController]
    [Route("connect")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly DatabaseContext _context;
        private readonly IAuthService _authService;

        public AuthorizationController(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager,
            DatabaseContext context,
            IAuthService authService)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Handles the OAuth 2.1 /connect/authorize endpoint.
        /// Performs user authentication checks, handles federated login redirects, and displays the user consent form.
        /// </summary>
        /// <returns>Redirects to the login flow, renders a consent screen, or signs in and redirects to the client application's callback URI.</returns>
        [HttpGet("authorize")]
        [HttpPost("authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.Features.Get<OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreFeature>()?.Transaction?.Request ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the user principal from the cookie authentication scheme.
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // If the user principal is not authenticated, redirect the user to the login page.
            if (!result.Succeeded)
            {
                return Challenge(
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    },
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme);
            }

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Retrieve the user identifier.
            var userId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { error = "invalid_user", error_description = "User identifier not found." });
            }

            // For OAuth 2.1, we require PKCE and standard consent.
            if (HttpMethods.IsPost(Request.Method) && Request.HasFormContentType)
            {
                if (Request.Form["consent"] == "denied")
                {
                    return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                if (Request.Form["consent"] != "approved")
                {
                    return BadRequest(new { error = "invalid_request", error_description = "Consent parameter is missing or invalid." });
                }
            }
            else
            {
                var appDisplayName = await _applicationManager.GetDisplayNameAsync(application);
                var scopes = string.Join(", ", request.GetScopes());
                
                // Build hidden inputs for all query parameters to preserve them in POST
                var hiddenInputs = string.Join("\n", Request.Query.Select(q => 
                    $"<input type='hidden' name='{System.Net.WebUtility.HtmlEncode(q.Key)}' value='{System.Net.WebUtility.HtmlEncode(q.Value)}' />"
                ));

                var consentHtml = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Authorization Consent</title>
                        <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap' rel='stylesheet'>
                        <style>
                            body {{ font-family: 'Inter', sans-serif; background-color: #0f172a; color: #f1f5f9; display: flex; align-items: center; justify-content: center; height: 100vh; margin: 0; }}
                            .card {{ background: rgba(30, 41, 59, 0.7); border: 1px solid rgba(255,255,255,0.1); border-radius: 12px; padding: 32px; width: 400px; text-align: center; backdrop-filter: blur(10px); box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                            h2 {{ margin-top: 0; color: #38bdf8; }}
                            p {{ color: #94a3b8; font-size: 14px; margin-bottom: 24px; }}
                            .scopes {{ background: rgba(15, 23, 42, 0.6); padding: 12px; border-radius: 6px; text-align: left; font-size: 13px; font-family: monospace; color: #a5f3fc; margin-bottom: 24px; }}
                            .btn {{ display: block; width: 100%; padding: 12px; border-radius: 6px; border: none; font-weight: 600; cursor: pointer; margin-bottom: 12px; transition: background 0.2s; }}
                            .btn-approve {{ background-color: #0284c7; color: white; }}
                            .btn-approve:hover {{ background-color: #0369a1; }}
                            .btn-deny {{ background-color: #dc2626; color: white; }}
                            .btn-deny:hover {{ background-color: #b91c1c; }}
                        </style>
                    </head>
                    <body>
                        <div class='card'>
                            <h2>Authorize App</h2>
                            <p><strong>{appDisplayName}</strong> is requesting permission to access your account.</p>
                            <div class='scopes'>Requested scopes: {scopes}</div>
                            <form method='post'>
                                {hiddenInputs}
                                <button type='submit' name='consent' value='approved' class='btn btn-approve'>Approve Access</button>
                                <button type='submit' name='consent' value='denied' class='btn btn-deny'>Deny Access</button>
                            </form>
                        </div>
                    </body>
                    </html>";
                return Content(consentHtml, "text/html");
            }

            // Create a new ClaimsIdentity
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: ClaimsIdentity.DefaultNameClaimType,
                roleType: ClaimsIdentity.DefaultRoleClaimType);

            var dbUser = int.TryParse(userId, out var parsedUserId)
                ? await _context.AuthUsers.FindAsync(parsedUserId)
                : null;
            var finalUsername = dbUser?.Username ?? result.Principal.Identity?.Name ?? "";

            identity.AddClaim(OpenIddictConstants.Claims.Subject, userId, OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(OpenIddictConstants.Claims.Name, finalUsername, OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim("userId", userId, OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim("userName", finalUsername, OpenIddictConstants.Destinations.AccessToken);

            var roles = result.Principal.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            foreach (var role in roles)
            {
                identity.AddClaim(OpenIddictConstants.Claims.Role, role, OpenIddictConstants.Destinations.AccessToken);
            }

            if (dbUser != null)
            {
                var userAppPermissions = await _authService.GetUserAppPermissions(parsedUserId);
                var appArray = userAppPermissions.Select(a => a.AppName).ToArray();
                identity.AddClaim("apps", Newtonsoft.Json.JsonConvert.SerializeObject(appArray), OpenIddictConstants.Destinations.AccessToken);
            }

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Rejects the authorization request, denying client access.
        /// </summary>
        /// <returns>Forbid result returning an access_denied OAuth 2.0 error response.</returns>
        [HttpPost("deny")]
        public IActionResult Deny()
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles the OAuth 2.1 /connect/token endpoint.
        /// Exchanges authorization codes or refresh tokens for JWT access tokens.
        /// </summary>
        /// <returns>A JSON response containing the access_token, id_token, and refresh_token.</returns>
        [HttpPost("token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.Features.Get<OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreFeature>()?.Transaction?.Request ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }
    }
}
