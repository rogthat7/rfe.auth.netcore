using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Shared;

namespace RFE.Auth.Core.Services
{
    public class WhatsAppSender : IWhatsAppSender
    {
        private readonly CommunicationServiceConfiguration _commConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsAppSender> _logger;

        public WhatsAppSender(
            IOptions<CommunicationServiceConfiguration> commConfig,
            IHttpClientFactory httpClientFactory,
            ILogger<WhatsAppSender> logger)
        {
            _commConfig = commConfig?.Value ?? throw new ArgumentNullException(nameof(commConfig));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendTextMessage(string toPhoneNumber, string message)
        {
            var payload = new
            {
                toPhoneNumber = toPhoneNumber,
                message = message,
                useTemplate = false
            };

            return await CallWhatsAppApiAsync(payload);
        }

        public async Task<bool> SendTemplateMessage(string toPhoneNumber, string templateName, string languageCode = null)
        {
            var payload = new
            {
                toPhoneNumber = toPhoneNumber,
                templateName = templateName,
                languageCode = languageCode,
                useTemplate = true
            };

            return await CallWhatsAppApiAsync(payload);
        }

        private async Task<bool> CallWhatsAppApiAsync(object payload)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CommunicationService");
                var url = $"{_commConfig.BaseUrl.TrimEnd('/')}/api/comm/v1/WhatsApp/send";

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp message sent successfully via Communication Service.");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Communication API (WhatsApp) returned error: {StatusCode} - {Body}", (int)response.StatusCode, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Communication Service WhatsApp endpoint.");
                return false;
            }
        }
    }
}
