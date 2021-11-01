using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly IAuthService _authService;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly ILogger<JwtAuthenticationService> _logger;

        public JwtAuthenticationService(IAuthService authService, IOptions<JwtOptions> jwtOptions, ILogger<JwtAuthenticationService> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest authenticateRequest)
        {
            var customOptionValues = _jwtOptions.Value;
            var user = await _authService.AuthenticateAuthUser(authenticateRequest);
            if(user==null)
                return null;
            var userAppPermissions = await _authService.GetUserAppPermissions(user.UserId.Value);
            if (user != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.ASCII.GetBytes(customOptionValues.Secret);
                var appArray = userAppPermissions.Select(a => a.AppName).ToArray<string>();
                var jtoken  = JToken.Parse(JsonConvert.SerializeObject(appArray));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]{
                        new Claim("role", "Admin"),
                        new Claim("userId", user.UserId.ToString()),
                        new Claim("userName", user.Username), 
                        new Claim("apps",JsonConvert.SerializeObject(appArray)),
                        new Claim("issuer",  customOptionValues.Issuer)
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
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