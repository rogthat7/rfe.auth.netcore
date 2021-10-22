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
    public class AddNewUserExample : IExamplesProvider<UnconfirmedAuthUser>
    {
        UnconfirmedAuthUser IExamplesProvider<UnconfirmedAuthUser>.GetExamples()
        {
            return new UnconfirmedAuthUser{
                Email = "iampokeman7@gmail.com",
                FirstName = "Roger",
                LastName = "Fernandes",
                Phone = 8806329362,
                Username = "iampokeman7@gmail.com",
                Password = "Password",
            
            };
        }
        
    }
}


