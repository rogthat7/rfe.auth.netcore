using System.Collections.Generic;
using System.Text;
using MimeKit;
using RFE.Auth.Core.Models.Email;
using Swashbuckle.AspNetCore.Filters;

namespace RFE.Auth.API.Models.Examples
{
    /// <summary>
    /// SendEmailExample
    /// </summary>
    public class SendEmailModelExample : IExamplesProvider<Message>
    {
        Message IExamplesProvider<Message>.GetExamples()
        {
            return new Message(new string[] {"iampokeman7@gmail.com"},"Test Email","This is Content for the email");
        }
        
    }
}


