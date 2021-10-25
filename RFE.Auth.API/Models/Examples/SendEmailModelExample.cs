using System.Collections.Generic;
using System.Text;
using MimeKit;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.User;
using Swashbuckle.AspNetCore.Filters;

namespace RFE.Auth.API.Models.Examples
{
    /// <summary>
    /// SendEmailExample
    /// </summary>
    public class SendEmailModelExample : IExamplesProvider<SendEmailDto>
    {
        SendEmailDto IExamplesProvider<SendEmailDto>.GetExamples()
        {
            return  new SendEmailDto(){
                    
                    Email = "iampokeman7@gmail.com",
                    FirstName = "Roger",
                    LastName = "Fernandes",
                    Password = "password",
                    Phone = 1231231231,
                    Username = "iampokeman7@gmail.com"
            };
        }
        
    }
}


