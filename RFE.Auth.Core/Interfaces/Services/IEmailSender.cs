using System.Threading.Tasks;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IEmailSender
    {
       Task<bool> SendUserConfirmationEmail(AuthUser user);
        
    }
}