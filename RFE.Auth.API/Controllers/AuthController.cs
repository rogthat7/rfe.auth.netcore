using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Models;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Email;

namespace RFE.Auth.API.Controllers
{
    /// <summary>
    /// AuthController
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtAuthenticationService _JwtAuthService;
        private readonly IMapper _mapper;
        /// <summary>
        /// AuthController
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="mapper"></param>
        public AuthController(IJwtAuthenticationService authService, IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _JwtAuthService = authService ?? throw new ArgumentNullException(nameof(authService));
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
    }
}
