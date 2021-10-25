using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Models.Examples;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
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
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly IAuthUserService _authuserService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        /// <summary>
        /// AuthUserController
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="authuserService"></param>
        /// <param name="mapper"></param>
        /// <param name="emailSender"></param>
        public UserController(IAuthService authService, IAuthUserService authuserService, IMapper mapper, IEmailSender emailSender)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _authuserService = authuserService ?? throw new ArgumentNullException(nameof(authuserService));
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
        // [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthUserGetResponseDto>>> GetAll()
        {
            var authusers = await _authuserService.GetAllRegisteredUsers();

            return Ok(new AppUserGetResponseDto(){
                Data = authusers, 
                Status = "OK"
            } );
        }
        /// <summary>
        /// GetAuthUserById
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(AuthUserGetResponseDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        // [Authorize]
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
        // [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuthUserByIdGetResponseDto>> AddAuthUser([FromBody] AuthUserAddPostRequestDto AuthUser)
        {
            var model = _mapper.Map<AuthUser>(AuthUser);
            model.Password = EncryptionHelper.EncodePasswordToBase64(model.Password);
            await _authuserService.AddNewAuthUser(model);

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
        // [Authorize]
        [HttpPost("sendconfirmationemail")]
        public async Task<ActionResult> SendConfirmationEmail([FromBody] SendEmailDto sendEmailDto)
        {
            var model = _mapper.Map<AuthUser>(sendEmailDto);
            var message = await _emailSender.SendUserConfirmationEmail(null);
            return Ok(message);
        }
    }
}
