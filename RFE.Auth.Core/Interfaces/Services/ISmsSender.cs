using System.Threading.Tasks;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface ISmsSender
    {
        Task<bool> SendUserConfirmationSms(AuthUser user, string verificationCode);
    }
}
