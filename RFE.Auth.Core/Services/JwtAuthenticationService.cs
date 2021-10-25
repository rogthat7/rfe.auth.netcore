using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly IAuthUserService _userService;
        private readonly IOptions<CustomOptions> _options;
        private readonly ILogger<JwtAuthenticationService> _logger;

        public JwtAuthenticationService(IAuthUserService userService, IOptions<CustomOptions> options, ILogger<JwtAuthenticationService> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest authenticateRequest)
        {
            var customOptionValues = _options.Value;
            var user = await _userService.AuthenticateAuthUser(authenticateRequest.Username, authenticateRequest.Password);
            if (user != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.ASCII.GetBytes(customOptionValues.JwtKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]{
                        new Claim("userName", user.Username), new Claim("apps","Fish-Tracker")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(tokenKey),
                            SecurityAlgorithms.HmacSha256Signature
                        )
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return new AuthenticateResponse(user, new Token
                {
                    type = "bearer",
                    value = tokenHandler.WriteToken(token)
                });
            }
            else return null;
        }
    }
}