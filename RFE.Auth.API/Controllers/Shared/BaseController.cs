using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RFE.Auth.API.Models.Shared;
using RFE.Auth.Core.Models.Shared;

namespace RFE.Auth.API.Controllers.Shared
{
    [Controller]
    public class BaseController : ControllerBase
    {
        private readonly IOptions<JwtOptions> _jwtOptions;

        public BaseController(IOptions<JwtOptions> jwtOptions)
        {
          _jwtOptions =   jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
        }
        
        protected bool ValidateToken(JwtSecurityToken token) 
        {
            //ToDo Verify with signiture
            bool issuerVerified = true;
            if (token.Issuer.CompareTo(_jwtOptions.Value.Issuer)!=0)
                issuerVerified = false;
           
            if(token.ValidTo <= DateTime.UtcNow)
                issuerVerified = false;
            return issuerVerified;
        }
    }
}