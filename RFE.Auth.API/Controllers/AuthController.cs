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
using RFE.Auth.Core.Models.Users;

namespace RFE.Auth.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;


        public AuthController(IAuthService authService, IUserService userService, UserContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
        public ActionResult<IEnumerable<AppUserGetResponseDto>> GetAll()
        {
            var users =  _mapper.Map<List<AppUserGetResponseDto>>(_context.AppUsers);
            return users;
            // var users = _userService.GetAll();
            // return Ok(users);
        }

    }
}
