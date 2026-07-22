using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RFE.Auth.API.Controllers.Shared;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Models.Examples;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RFE.Auth.Core.Models.App;
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
        private readonly IAuthService _authService;
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// UserController
        /// </summary>
        /// <param name="authuserService"></param>
        /// <param name="jwtAuthService"></param>
        /// <param name="mapper"></param>
        /// <param name="emailSender"></param>
        /// <param name="smsSender"></param>
        /// <param name="jwtoptions"></param>
        /// <param name="authService"></param>
        public UserController(  IUserService authuserService,
                                 IJwtAuthenticationService jwtAuthService, 
                                 IMapper mapper,
                                 IEmailSender emailSender,
                                 ISmsSender smsSender,
                                 IOptions<JwtOptions> jwtoptions,
                                 IAuthService authService,
                                 DatabaseContext dbContext,
                                 ILogger<UserController> logger): base (jwtoptions)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _authuserService = authuserService ?? throw new ArgumentNullException(nameof(authuserService));
            _JwtAuthService = jwtAuthService ?? throw new ArgumentNullException(nameof(jwtAuthService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
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

        /// <summary>
        /// Sends a 6-digit verification code to the user's phone.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("sendconfirmationphone")]
        public async Task<IActionResult> SendConfirmationPhone([FromBody] SendPhoneConfirmationRequest model)
        {
            if (model == null || model.Phone == 0)
                return BadRequest(new { success = false, message = "Phone is required." });

            var username = model.Username ?? model.Phone.ToString();
            var role = model.Role ?? "appUser";
            var appId = model.AppId ?? "rfe-glam-app";

            var userDto = new AuthUserAddPostRequestDto
            {
                Username = username,
                Password = model.Password,
                Phone = model.Phone,
                Email = null
            };

            var mappedUser = _mapper.Map<AuthUser>(userDto);
            mappedUser.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);

            // Generate 6-digit OTP code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Store user info and code in JWT payload
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] {
                new Claim("payload", JsonConvert.SerializeObject(mappedUser)),
                new Claim("code", code),
                new Claim("role", role),
                new Claim("app", appId)
            };

            var otpToken = new JwtSecurityToken(
                _jwtOptions.Value.Issuer, null, claims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials);
            var jwtPayload = new JwtSecurityTokenHandler().WriteToken(otpToken);

            // Send confirmation code via SMS
            var smsSent = await _smsSender.SendUserConfirmationSms(mappedUser, code);

            return Ok(new
            {
                success = true,
                verificationMethod = "phone",
                tokenPayload = jwtPayload,
                devOtp = smsSent ? null : code,
                message = smsSent
                    ? "Verification code sent via SMS"
                    : "SMS sending failed (not configured) — use devOtp for testing",
                status = "OK"
            });
        }

        /// <summary>
        /// Sends a 6-digit verification code to the user's email.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("sendconfirmationemail")]
        public async Task<IActionResult> SendConfirmationEmail([FromBody] SendEmailConfirmationRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email))
                return BadRequest(new { success = false, message = "Email is required." });

            var username = model.Email;
            var role = model.Role ?? "appUser";
            var appId = model.AppId ?? "rfe-glam-app";

            // Check if username/email is already taken (bypassed to allow OTP verification/resend & Glam integration for existing users)

            var userDto = new AuthUserAddPostRequestDto
            {
                Username = username,
                Password = model.Password,
                Email = model.Email
            };

            var mappedUser = _mapper.Map<AuthUser>(userDto);
            mappedUser.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);

            // Generate 6-digit OTP code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Store user info and code in JWT payload
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] {
                new Claim("payload", JsonConvert.SerializeObject(mappedUser)),
                new Claim("code", code),
                new Claim("role", role),
                new Claim("app", appId)
            };

            var otpToken = new JwtSecurityToken(
                _jwtOptions.Value.Issuer, null, claims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials);
            var jwtPayload = new JwtSecurityTokenHandler().WriteToken(otpToken);

            // Send confirmation code via email
            var displayName = appId?.ToLowerInvariant() switch
            {
                "fish-tracker" or "fish-tracker-app" => "Fish Tracker",
                "rfe-glam-app" or "rfe-glam" => "RFE Glam",
                _ => "RFE Auth"
            };
            var subject = $"Confirm your {displayName} account";
            var body = $"Your verification code is: <strong>{code}</strong>. It will expire in 10 minutes.";

            var emailSent = await _emailSender.SendGeneralEmail(model.Email, subject, body);

            return Ok(new
            {
                success = true,
                verificationMethod = "email",
                tokenPayload = jwtPayload,
                devOtp = emailSent ? null : code,
                message = emailSent
                    ? "Verification code sent via email"
                    : "Email sending failed (SMTP configuration error) — use devOtp for testing"
            });
        }

        /// <summary>
        /// ConfirmUserWithEmail — validates the OTP code and activates the user account.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("confirmuserwithemail")]
        public async Task<ActionResult> ConfirmUserWithEmail([FromBody] ConfirmEmailRequest request)
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
                message = "Email verified! Account created successfully.",
                status = "OK"
            });
        }


        /// <summary>
        /// Gets a JWT token for the currently cookie-authenticated user.
        /// </summary>
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet("token")]
        public async Task<IActionResult> GetTokenFromCookie()
        {
            var userIdVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdVal) || !Guid.TryParse(userIdVal, out var userId))
            {
                return Unauthorized(new { message = "User not authenticated or invalid identifier" });
            }

            var user = await _authuserService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userAppPermissions = await _authService.GetUserAppPermissions(userId);
            var role = await _authService.GetUserRoleAsync(userId);

            var customOptionValues = _jwtOptions.Value;
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(customOptionValues.Secret);
            var appArray = userAppPermissions.Select(a => a.AppName).ToArray<string>();
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim("role", role ?? "appUser"),
                    new Claim("userId", user.UserId.ToString()),
                    new Claim("userName", user.Username), 
                    new Claim("apps", JsonConvert.SerializeObject(appArray)),
                    new Claim("issuer", customOptionValues.Issuer)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = new
                {
                    value = tokenHandler.WriteToken(token)
                }
            });
        }

        /// <summary>
        /// Callback page for successful federated authentication that notifies its opener and closes itself.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("oauth-success")]
        public IActionResult OAuthSuccess()
        {
            var html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Authentication Success</title>
                    <style>
                        body {
                            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            background-color: #0f172a;
                            color: #f1f5f9;
                            display: flex;
                            flex-direction: column;
                            align-items: center;
                            justify-content: center;
                            height: 100vh;
                            margin: 0;
                            text-align: center;
                        }
                        .container {
                            background: rgba(30, 41, 59, 0.7);
                            border: 1px solid rgba(255, 255, 255, 0.1);
                            border-radius: 12px;
                            padding: 32px;
                            backdrop-filter: blur(10px);
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                            max-width: 320px;
                        }
                        h2 { color: #10b981; margin-top: 0; }
                        p { color: #94a3b8; font-size: 14px; margin-bottom: 0; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Success!</h2>
                        <p>You have authenticated successfully. This window will now close.</p>
                    </div>
                    <script>
                        if (window.opener) {
                            window.opener.postMessage({ type: 'oauth-success' }, window.location.origin);
                            window.close();
                        } else {
                            window.location.href = '/scalar/v1';
                        }
                    </script>
                </body>
                </html>";
            return Content(html, "text/html");
        }

        /// <summary>
        /// RequestPasswordRecovery — requests a code for password recovery.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("recover-password/request")]
        public async Task<IActionResult> RequestPasswordRecovery([FromBody] PasswordRecoveryRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Identifier) || string.IsNullOrWhiteSpace(model.Method) || string.IsNullOrWhiteSpace(model.AppId))
                return BadRequest(new { success = false, message = "Identifier, Method, and AppId are required." });

            var method = model.Method.ToLower();
            if (method != "email" && method != "phone")
                return BadRequest(new { success = false, message = "Method must be 'email' or 'phone'." });

            // Ensure only registered apps can make these requests
            var app = await _dbContext.Apps.FirstOrDefaultAsync(a => a.AppName == model.AppId);
            if (app == null)
            {
                _logger.LogWarning("Unauthorized password recovery request from unregistered app: {AppId}", model.AppId);
                return Unauthorized(new { success = false, message = $"Application '{model.AppId}' is not registered." });
            }

            // Track request
            _logger.LogInformation("Password recovery requested. App: {AppName} ({DisplayName}), Method: {Method}, Identifier: {Identifier}", 
                app.AppName, app.DisplayName, method, model.Identifier);

            var rawIdentifier = ExtractFirstRecipient(model.Identifier);

            // Find user
            AuthUser user = null;
            if (method == "email")
            {
                user = await _dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Email == rawIdentifier);
            }
            else // phone
            {
                if (long.TryParse(rawIdentifier.Replace(" ", "").Replace("\t", ""), out long phoneLong))
                {
                    user = await _dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Phone == phoneLong);
                }
            }

            // Return appropriate error if the user doesn't exist
            if (user == null)
            {
                _logger.LogWarning("Password recovery requested for non-existent user: {Identifier} via {Method} on App: {AppId}", model.Identifier, method, model.AppId);
                var errorMsg = method == "email" 
                    ? "No user found with the provided email address." 
                    : "No user found with the provided phone number.";
                return NotFound(new { success = false, message = errorMsg });
            }

            // Generate 6-digit OTP code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Store user info and code in signed JWT payload
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] {
                new Claim("userId", user.UserId.ToString()),
                new Claim("code", code),
                new Claim("app", model.AppId)
            };

            var otpToken = new JwtSecurityToken(
                _jwtOptions.Value.Issuer, null, claims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);
            var jwtPayload = new JwtSecurityTokenHandler().WriteToken(otpToken);

            // Send confirmation code
            bool sentSuccess = false;
            if (method == "email")
            {
                var subject = $"{app.DisplayName} Password Recovery";
                var emailBody = $"Your password recovery code is: {code}";
                sentSuccess = await _emailSender.SendGeneralEmail(user.Email, subject, emailBody);
            }
            else // phone
            {
                var smsMessage = $"Your {app.DisplayName} password recovery code is: {code}";
                sentSuccess = await _smsSender.SendGeneralSms(user.Phone.ToString(), smsMessage);
            }

            return Ok(new
            {
                success = true,
                message = "If the account exists, a recovery code has been sent.",
                tokenPayload = jwtPayload,
                devOtp = sentSuccess ? null : code // Expose OTP in dev mode if sending fails/is mock
            });
        }

        /// <summary>
        /// ResetPassword — resets the user's password using the recovery code.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("recover-password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TokenPayload) || string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { success = false, message = "TokenPayload, Code, and NewPassword are required." });

            try
            {
                // Verify JWT token payload
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail)),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Value.Issuer,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal;
                try
                {
                    principal = tokenHandler.ValidateToken(model.TokenPayload, validationParameters, out SecurityToken validatedToken);
                }
                catch (Exception)
                {
                    return BadRequest(new { success = false, message = "Invalid or expired recovery token." });
                }

                var codeClaim = principal.FindFirst("code")?.Value;
                var userIdClaim = principal.FindFirst("userId")?.Value;
                var appClaim = principal.FindFirst("app")?.Value;

                if (string.IsNullOrEmpty(codeClaim) || string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(appClaim))
                    return BadRequest(new { success = false, message = "Malformed recovery token." });

                if (model.Code != codeClaim)
                    return BadRequest(new { success = false, message = "Invalid recovery code." });

                // Check registered app check again
                var app = await _dbContext.Apps.FirstOrDefaultAsync(a => a.AppName == appClaim);
                if (app == null)
                {
                    return Unauthorized(new { success = false, message = $"Application '{appClaim}' is not registered." });
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                    return BadRequest(new { success = false, message = "Invalid user identification in token." });

                var user = await _dbContext.AuthUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });

                // Update password
                user.Password = EncryptionHelper.EncodePasswordToBase64(model.NewPassword);
                _dbContext.AuthUsers.Update(user);
                await _dbContext.SaveChangesAsync();

                // Track the successful reset
                _logger.LogInformation("Password successfully reset for UserId: {UserId} via App: {AppId}", user.UserId, appClaim);

                return Ok(new { success = true, message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset.");
                return StatusCode(500, new { success = false, message = "An error occurred while resetting your password." });
            }
        }

        private static string ExtractFirstRecipient(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            char[] delimiters = new[] { ';', ',', '\r', '\n', '\t', ' ' };
            var parts = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0].Trim() : string.Empty;
        }
    }

    public class PasswordRecoveryRequest
    {
        public string Identifier { get; set; } // Email or Phone number
        public string Method { get; set; }     // "email" or "phone"
        public string AppId { get; set; }      // Application identifier, e.g. "rfe-glam-app"
    }

    public class PasswordResetRequest
    {
        public string TokenPayload { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
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

    public class SendEmailConfirmationRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string AppId { get; set; }
    }

    public class SendPhoneConfirmationRequest
    {
        public string Username { get; set; }
        public long Phone { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string? AppId { get; set; }
    }

    public class ConfirmEmailRequest
    {
        public string TokenPayload { get; set; }
        public string Code { get; set; }
    }
}
