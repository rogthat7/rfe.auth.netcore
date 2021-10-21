using System.Threading.Tasks;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Email;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IEmailSender
    {
       Task SendEmail(Message message);
        
    }
}