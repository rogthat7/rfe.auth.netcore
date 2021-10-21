using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuthUserByIdGetResponseDto>> AddAuthUser([FromBody] AuthUserAddPostRequestDto AuthUser)
        {
            var model = _mapper.Map<UnconfirmedAuthUser>(AuthUser);
            var authuser = await _authuserService.AddNewAuthUser(model);

            return Ok(new AuthUserByIdGetResponseDto(){
                Data = authuser, 
                Status = "OK"
            } );
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize]
        [HttpPost("authusers/sendemail")]
        public async Task<ActionResult> SendEmailTest([FromBody] Message emailMessage)
        {
            await _emailSender.SendEmail(emailMessage);

            return Ok();
        }
    }
}
