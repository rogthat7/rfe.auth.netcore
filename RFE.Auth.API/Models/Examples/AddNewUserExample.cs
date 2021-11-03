using System.Collections.Generic;
using System.Text;
using MimeKit;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.User;
using Swashbuckle.AspNetCore.Filters;

namespace RFE.Auth.API.Models.Examples
{
    /// <summary>
    /// SendEmailExample
    /// </summary>
    public class AddNewUserExample : IExamplesProvider<AuthUser>
    {
        AuthUser IExamplesProvider<AuthUser>.GetExamples()
        {
            return new AuthUser{
                Email = "iampokeman7@gmail.com",
                Phone = 8806329362,
                Username = "iampokeman7@gmail.com",
                Password = "Password"
            };
        }
        
    }
}


