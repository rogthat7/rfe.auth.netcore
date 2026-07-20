using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly CommunicationServiceConfiguration _commConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SmsSender> _logger;

        public SmsSender(
            IOptions<CommunicationServiceConfiguration> commConfig,
            IHttpClientFactory httpClientFactory,
            ILogger<SmsSender> logger)
        {
            _commConfig = commConfig?.Value ?? throw new ArgumentNullException(nameof(commConfig));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendUserConfirmationSms(AuthUser user, string verificationCode)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var phone = user.Phone.HasValue ? user.Phone.Value.ToString() : "Unknown";
            var message = $"SMS Verification code for {user.Username} ({phone}): {verificationCode}";
            
            return await SendGeneralSms(phone, message);
        }

        public async Task<bool> SendGeneralSms(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            var cleanPhone = phoneNumber.Replace(" ", "").Replace("\t", "");
            if (!cleanPhone.StartsWith("+") && cleanPhone != "Unknown")
            {
                cleanPhone = "+" + cleanPhone;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CommunicationService");
                var url = $"{_commConfig.BaseUrl.TrimEnd('/')}/api/comm/v1/Sms/send";

                var payload = new
                {
                    phoneNumber = cleanPhone,
                    message = message
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS sent successfully via Communication Service.");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Communication API (SMS) returned error: {StatusCode} - {Body}", (int)response.StatusCode, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Communication Service SMS endpoint.");
                return false;
            }
        }
    }
}
