using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly ILogger<SmsSender> _logger;

        public SmsSender(ILogger<SmsSender> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<bool> SendUserConfirmationSms(AuthUser user, string verificationCode)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var phone = user.Phone.HasValue ? user.Phone.Value.ToString() : "Unknown";
            _logger.LogInformation("==================================================");
            _logger.LogInformation("SMS Verification code for {Username} ({Phone}): {Code}", user.Username, phone, verificationCode);
            _logger.LogInformation("==================================================");

            return Task.FromResult(true);
        }
    }
}
