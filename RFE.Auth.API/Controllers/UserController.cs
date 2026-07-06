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
        /// AuthUserController
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="authuserService"></param>
        /// <param name="mapper"></param>
        /// <param name="emailSender"></param>
        /// <param name="smsSender"></param>
        public UserController(  IAuthService authService, 
                                IUserService authuserService,
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
        public async Task<ActionResult<AuthUserByIdGetResponseDto>> GetAuthUserById([FromRoute] int id)
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
        /// SendEmailTest
        /// </summary>
        /// <param name="emailMessage"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(SendEmailDto), typeof(SendEmailModelExample))]
        [Authorize]
        [HttpPost("sendconfirmationemail")]
        public async Task<ActionResult> SendConfirmationEmail([FromBody] SendEmailDto sendEmailDto)
        {
            var model = _mapper.Map<AuthUser>(sendEmailDto);
            model.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);
            var message = await _emailSender.SendUserConfirmationEmail(model);
            return Ok(message);
        }
        /// <summary>
        /// SendEmailTest
        /// </summary>
        /// <param name="emailMessage"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(SendEmailDto), typeof(string))]
        [AllowAnonymous]
        [HttpGet("confirmuserwithconfirmationlink")]
        public async Task<ActionResult> SendConfirmationEmail([FromQuery] string tokenPayload)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(tokenPayload);
            
            if(!ValidateToken(jwtSecurityToken))
                return BadRequest("Invalid Token, Please Register Again");
            var payload = jwtSecurityToken.Payload.First(data => data.Key == "payload").Value;
            var model = JsonConvert.DeserializeObject<AuthUser>(payload.ToString());
            await _authuserService.AddNewAuthUser(model, "appUser");

            return Ok(new AuthUserAddPostResponseDto()
            {
                Message = "New Auth User Added",
                Status = "OK"
            });
        }

        /// <summary>
        /// SendConfirmationPhone
        /// </summary>
        /// <param name="sendPhoneDto"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpPost("sendconfirmationphone")]
        public async Task<ActionResult> SendConfirmationPhone([FromBody] SendPhoneDto sendPhoneDto)
        {
            var model = _mapper.Map<AuthUser>(sendPhoneDto);
            model.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);

            // Generate a random 6-digit code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Store user details and verification code in JWT payload
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] {
                new System.Security.Claims.Claim("payload", JsonConvert.SerializeObject(model)),
                new System.Security.Claims.Claim("code", code),
                new System.Security.Claims.Claim("role", sendPhoneDto.Role ?? "appUser")
            };
            
            var token = new JwtSecurityToken(
                _jwtOptions.Value.Issuer,
                null,
                claims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(5), // 5 minutes validity
                signingCredentials: credentials
            );
            
            var jwtPayLoad = new JwtSecurityTokenHandler().WriteToken(token);

            // Send confirmation SMS
            var messageSent = await _smsSender.SendUserConfirmationSms(model, code);
            if (!messageSent)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error sending SMS");
            }

            return Ok(new 
            {
                Message = "Verification code sent to phone number",
                TokenPayload = jwtPayLoad,
                Status = "OK"
            });
        }

        /// <summary>
        /// ConfirmUserWithPhone
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

            var model = JsonConvert.DeserializeObject<AuthUser>(payloadClaim);
            await _authuserService.AddNewAuthUser(model, roleClaim);

            return Ok(new AuthUserAddPostResponseDto()
            {
                Message = "New Auth User Added Successfully via Phone Verification",
                Status = "OK"
            });
        }

    }
}
