using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RFE.Auth.API.Controllers.Shared;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Models.Examples;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;
using Swashbuckle.AspNetCore.Filters;

namespace RFE.Auth.API.Controllers
{
    /// <summary>
    /// UserController
    /// </summary>
    [ApiController]
    [Route("api/auth/v1/[controller]")]
    public class UserController : BaseController 
    {
        private readonly IUserService _authuserService;
        private readonly IMapper _mapper;
        private readonly IJwtAuthenticationService _JwtAuthService;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        /// <summary>
        /// UserController
        /// </summary>
        /// <param name="authuserService"></param>
        /// <param name="jwtAuthService"></param>
        /// <param name="mapper"></param>
        /// <param name="emailSender"></param>
        /// <param name="smsSender"></param>
        /// <param name="jwtoptions"></param>
        public UserController(  IUserService authuserService,
                                 IJwtAuthenticationService jwtAuthService, 
                                 IMapper mapper,
                                 IEmailSender emailSender,
                                 ISmsSender smsSender,
                                 IOptions<JwtOptions> jwtoptions): base (jwtoptions)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _authuserService = authuserService ?? throw new ArgumentNullException(nameof(authuserService));
            _JwtAuthService = jwtAuthService ?? throw new ArgumentNullException(nameof(jwtAuthService));
        } 
        /// <summary>
        /// Authenticate
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            model.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);
            var response = await _JwtAuthService.Authenticate(model);

            if (response == null)
                return Unauthorized(new { message = "Username or password is incorrect" });

            return Ok(response);
        }
        /// <summary>
        /// GetAll Users
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<AuthUserGetResponseDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthUserAuthenticateResponseDto>>> GetAll()
        {
            var authusers = await _authuserService.GetAllRegisteredUsers();

            return Ok(new AuthUserGetResponseDto(){
                Data = authusers, 
                Status = "OK"
            } );
        }
        /// <summary>
        /// GetAuthUserById
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(AuthUserAuthenticateResponseDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthUserByIdGetResponseDto>> GetAuthUserById([FromRoute] Guid id)
        {
            var authuser = await _authuserService.GetUserById(id);

            return Ok(new AuthUserByIdGetResponseDto(){
                Data = authuser, 
                Status = "OK"
            } );
        }

        /// <summary>
        /// AddAuthUser
        /// </summary>
        /// <param name="AuthUser"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(AuthUserAddPostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(AuthUser), typeof(AddNewUserExample))]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuthUserByIdGetResponseDto>> AddAuthUser([FromBody] AuthUserAddPostRequestDto AuthUser)
        {
            var model = _mapper.Map<AuthUser>(AuthUser);
            model.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);
            await _authuserService.AddNewAuthUser(model, "appUser");

            return Ok(new AuthUserAddPostResponseDto()
            {
                Message = "New Auth User Added",
                Status = "OK"
            });

        }

        /// <summary>
        /// Compatibility login endpoint for frontend requests.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("/api/auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var identifier = !string.IsNullOrEmpty(model.Username) ? model.Username
                           : !string.IsNullOrEmpty(model.Phone) ? model.Phone : model.Email;
            if (string.IsNullOrEmpty(identifier))
                return BadRequest(new { success = false, message = "Username, phone or email is required." });

            var encryptedPassword = EncryptionHelper.EncodePasswordToBase64(model.Password);
            var authReq = new AuthenticateRequest
            {
                Username = identifier,
                Password = encryptedPassword
            };
            
            var authResponse = await _JwtAuthService.Authenticate(authReq);
            if (authResponse == null)
            {
                return Unauthorized(new { success = false, message = "Username or password is incorrect" });
            }

            // Check if user is verified
            if (!authResponse.User.IsVerified)
            {
                return StatusCode(403, new
                {
                    success = false,
                    verified = false,
                    message = "Account not verified. Please verify your email or phone to continue.",
                    identifier = identifier
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, authResponse.User.UserId?.ToString() ?? ""),
                new Claim(ClaimTypes.Name, authResponse.User.Username),
                new Claim(ClaimTypes.Email, authResponse.User.Email ?? ""),
                new Claim(ClaimTypes.Role, "appUser")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    token = authResponse.Token.value,
                    refreshToken = (string)null,
                    expiresAt = DateTime.UtcNow.AddDays(1).ToString("o")
                }
            });
        }

        /// <summary>
        /// Compatibility register endpoint for frontend requests.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("/api/auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Phone) && string.IsNullOrEmpty(model.Email))
                return BadRequest(new { success = false, message = "At least one of Phone or Email is required." });

            var username = !string.IsNullOrEmpty(model.Username) ? model.Username
                         : !string.IsNullOrEmpty(model.Phone) ? model.Phone : model.Email;
            var role     = model.Role   ?? "appUser";
            var appId    = model.AppId  ?? "rfe-auth";

            long? phoneVal = long.TryParse(model.Phone, out long parsedPhone) ? parsedPhone : (long?)null;

            // Check if username, email, or phone is already taken
            var existingUsers = await _authuserService.GetAllRegisteredUsers();
            if (existingUsers != null)
            {
                if (existingUsers.Any(u => !string.IsNullOrEmpty(u.Username) && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { success = false, message = "Username is already taken." });
                }
                if (!string.IsNullOrEmpty(model.Email) && existingUsers.Any(u => !string.IsNullOrEmpty(u.Email) && u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { success = false, message = "Email is already registered." });
                }
                if (phoneVal.HasValue && existingUsers.Any(u => u.Phone == phoneVal.Value))
                {
                    return BadRequest(new { success = false, message = "Phone number is already registered." });
                }
            }

            var userDto = new AuthUserAddPostRequestDto
            {
                Username = username,
                Password = model.Password,
                Phone    = phoneVal,
                Email    = !string.IsNullOrEmpty(model.Email) ? model.Email : null
            };

            var mappedUser = _mapper.Map<AuthUser>(userDto);
            mappedUser.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);

            // PHONE — send OTP verification
            if (!string.IsNullOrEmpty(model.Phone))
            {
                var random = new Random();
                var code   = random.Next(100000, 999999).ToString();

                var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
                var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature);

                var claims = new[] {
                    new System.Security.Claims.Claim("payload", JsonConvert.SerializeObject(mappedUser)),
                    new System.Security.Claims.Claim("code",    code),
                    new System.Security.Claims.Claim("role",    role),
                    new System.Security.Claims.Claim("app",     appId)
                };
                var otpToken = new JwtSecurityToken(
                    _jwtOptions.Value.Issuer, null, claims,
                    DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: credentials);
                var jwtPayload = new JwtSecurityTokenHandler().WriteToken(otpToken);

                // Attempt SMS — dev fallback: include code in response if sender fails
                var smsSent = await _smsSender.SendUserConfirmationSms(mappedUser, code);

                return Ok(new
                {
                    success = true,
                    verificationMethod = "phone",
                    tokenPayload = jwtPayload,
                    // Dev helper: expose OTP if SMS not configured
                    devOtp = smsSent ? null : code,
                    message = smsSent
                        ? "Verification code sent via SMS"
                        : "SMS not configured — use devOtp for testing"
                });
            }

            // EMAIL-ONLY — send email link
            var emailSecKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
            var emailCreds  = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                emailSecKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature);

            var emailClaims = new[] {
                new System.Security.Claims.Claim("payload", JsonConvert.SerializeObject(mappedUser)),
                new System.Security.Claims.Claim("role",    role),
                new System.Security.Claims.Claim("app",     appId)
            };
            var emailToken = new JwtSecurityToken(
                _jwtOptions.Value.Issuer, null, emailClaims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: emailCreds);
            var emailJwt = new JwtSecurityTokenHandler().WriteToken(emailToken);

            var emailModel = new AuthUser { Username = username, Email = model.Email, Password = mappedUser.Password };
            var emailSent = await _emailSender.SendUserConfirmationEmail(emailModel, role, appId);

            var devLink = emailSent ? null : $"https://localhost:5001/api/auth/v1/User/confirmuserwithconfirmationlink?tokenPayload={emailJwt}";

            return Ok(new
            {
                success = true,
                verificationMethod = "email",
                emailSent = emailSent,
                devLink = devLink,
                message = emailSent
                    ? "Verification email sent. Please check your inbox."
                    : "Email sending failed (SMTP configuration error) — use devLink for testing"
            });
        }

        /// <summary>
        /// Compatibility logout endpoint for frontend requests.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("/api/auth/logout")]
        public IActionResult Logout()
        {
            return Ok(new { Message = "Logged out successfully" });
        }

        /// <summary>
        /// Resend verification — phone or email.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("/api/auth/resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest model)
        {
            if (string.IsNullOrEmpty(model.Identifier))
                return BadRequest(new { success = false, message = "Identifier required." });

            // Re-use Register logic by constructing a minimal RegisterRequestModel
            var registerModel = new RegisterRequestModel
            {
                Phone    = model.Method == "phone" ? model.Identifier : null,
                Email    = model.Method == "email" ? model.Identifier : null,
                Password = model.Password ?? "placeholder",
                Role     = "appUser",
                AppId    = "rfe-auth"
            };
            return await Register(registerModel);
        }

        /// <summary>
        /// ConfirmUserWithEmailLink — validates the JWT confirmation link from the email and activates the user.
        /// </summary>
        /// <param name="tokenPayload"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("confirmuserwithconfirmationlink")]
        public async Task<ActionResult> ConfirmUserWithEmailLink([FromQuery] string tokenPayload)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken;
            try { jwtSecurityToken = handler.ReadJwtToken(tokenPayload); }
            catch { return BadRequest("Invalid token payload"); }

            if (!ValidateToken(jwtSecurityToken))
                return BadRequest("Token expired or invalid. Please register again.");

            var payload = jwtSecurityToken.Payload.FirstOrDefault(data => data.Key == "payload").Value?.ToString();
            var roleClaim = jwtSecurityToken.Payload.FirstOrDefault(data => data.Key == "role").Value?.ToString() ?? "appUser";
            var appClaim  = jwtSecurityToken.Payload.FirstOrDefault(data => data.Key == "app").Value?.ToString()  ?? "rfe-auth";

            if (string.IsNullOrEmpty(payload))
                return BadRequest("Invalid token content");

            var model = JsonConvert.DeserializeObject<AuthUser>(payload);
            try
            {
                await _authuserService.AddNewAuthUser(model, roleClaim, appClaim);
                await _authuserService.MarkUserAsVerified(model.Username);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("23505") || ex.InnerException?.Message.Contains("23505") == true ||
                    ex.Message.Contains("unique constraint") || ex.InnerException?.Message.Contains("unique constraint") == true)
                {
                    return BadRequest("User already exists or has been verified by another session.");
                }
                throw;
            }

            return Redirect("http://localhost:3001/verify-email?status=confirmed");
        }

        /// <summary>
        /// ConfirmUserWithPhone — validates the OTP code and activates the user account.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpPost("confirmuserwithphone")]
        public async Task<ActionResult> ConfirmUserWithPhone([FromBody] ConfirmPhoneRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.TokenPayload) || string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Invalid request parameters");
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = handler.ReadJwtToken(request.TokenPayload);
            }
            catch (Exception)
            {
                return BadRequest("Invalid token payload");
            }

            if (!ValidateToken(jwtSecurityToken))
            {
                return BadRequest("Token expired or invalid, please register again");
            }

            // Verify code
            var codeClaim = jwtSecurityToken.Payload.FirstOrDefault(data => data.Key == "code").Value?.ToString();
            if (codeClaim != request.Code)
            {
                return BadRequest("Verification code is incorrect");
            }

            var payloadClaim = jwtSecurityToken.Payload.FirstOrDefault(data => data.Key == "payload").Value?.ToString();
            if (string.IsNullOrEmpty(payloadClaim))
            {
                return BadRequest("Invalid token content");
            }

            var roleClaim = jwtSecurityToken.Payload.FirstOrDefault(data => data.Key == "role").Value?.ToString() ?? "appUser";
            var appClaim = jwtSecurityToken.Payload.FirstOrDefault(data => data.Key == "app").Value?.ToString() ?? "rfe-auth";

            var model = JsonConvert.DeserializeObject<AuthUser>(payloadClaim);
            try
            {
                await _authuserService.AddNewAuthUser(model, roleClaim, appClaim);
                await _authuserService.MarkUserAsVerified(model.Username);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("23505") || ex.InnerException?.Message.Contains("23505") == true ||
                    ex.Message.Contains("unique constraint") || ex.InnerException?.Message.Contains("unique constraint") == true)
                {
                    return BadRequest("User already exists or has been verified by another session.");
                }
                throw;
            }

            return Ok(new
            {
                success = true,
                message = "Phone verified! Account created successfully.",
                status = "OK"
            });
        }
    }

    public class LoginRequestModel
    {
        public string Username { get; set; }
        public string Phone    { get; set; }
        public string Email    { get; set; }
        public string Password { get; set; }
        public string AppId    { get; set; }
    }

    public class RegisterRequestModel
    {
        public string Username { get; set; }
        public string Name     { get; set; }
        public string Phone    { get; set; }
        public string Email    { get; set; }
        public string Password { get; set; }
        public string Role     { get; set; }
        public string AppId    { get; set; }
    }

    public class ResendVerificationRequest
    {
        public string Identifier { get; set; }
        public string Method     { get; set; } // "phone" or "email"
        public string Password   { get; set; }
    }
}
