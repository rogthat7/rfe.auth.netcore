using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RFE.Auth.API.Models;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Email;

namespace RFE.Auth.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public AuthController(IAuthService authService, IUserService userService, IMapper mapper, IEmailSender emailSender)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _authService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize]
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<AppUserGetResponseDto>>> GetAll()
        {
            var users = await _userService.GetAllRegisteredUsers();

            return Ok(new AppUserGetResponseDto(){
                Data = users, 
                Status = "OK"
            } );
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize]
        [HttpGet("users/{id}")]
        public async Task<ActionResult<IEnumerable<AppUserGetResponseDto>>> GetUserById([FromRoute] int id)
        {
            var user = await _userService.GetUserById(id);

            return Ok(new AppUserByIdGetResponseDto(){
                Data = user, 
                Status = "OK"
            } );
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize]
        [HttpPost("users/sendemail")]
        public async Task<ActionResult<IEnumerable<AppUserGetResponseDto>>> SendEmailTest([FromBody] Message emailMessage)
        {
            await _emailSender.SendEmail(emailMessage);

            return Ok(new AppUserByIdGetResponseDto(){
                Data = null, 
                Status = "OK"
            } );
        }
    }
}
