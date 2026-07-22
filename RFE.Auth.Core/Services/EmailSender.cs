using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace RFE.Auth.Core.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly CommunicationServiceConfiguration _commConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly ILogger<EmailSender> _logger;
        private readonly string DEFAULT_USER_CONFIRMATION_SUBJECT = "User Registration Confirmation";

        public EmailSender(
            IOptions<CommunicationServiceConfiguration> commConfig, 
            IHttpClientFactory httpClientFactory, 
            ILogger<EmailSender> logger, 
            IOptions<JwtOptions> jwtOptions)
        {
            _commConfig = commConfig?.Value ?? throw new ArgumentNullException(nameof(commConfig));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendUserConfirmationEmail(AuthUser authUser, string role = "appUser", string appId = "rfe-auth")
        {
            try
            {
                var htmlBody = await GetEmailBody(authUser, role, appId);
                var baseDir = AppContext.BaseDirectory;
                var inlineAttachments = new List<InlineAttachmentDto>();

                // List of inline images to attach
                var imageFiles = new[] { "image-1.png", "image-2.png", "image-3.png", "image-4.png", "image-5.png", "image-6.png" };
                var cidMap = new Dictionary<string, string>();

                foreach (var img in imageFiles)
                {
                    var imgPath = Path.Combine(baseDir, "Resources", "images", img);
                    if (File.Exists(imgPath))
                    {
                        var contentId = Guid.NewGuid().ToString("N");
                        var base64 = Convert.ToBase64String(await File.ReadAllBytesAsync(imgPath));
                        
                        inlineAttachments.Add(new InlineAttachmentDto
                        {
                            ContentId = contentId,
                            FileName = img,
                            ContentBase64 = base64,
                            ContentType = "image/png"
                        });

                        cidMap.Add(img, contentId);
                    }
                }

                // Replace references to images/image-X.png with cid:ContentId
                foreach (var pair in cidMap)
                {
                    htmlBody = htmlBody.Replace($"images/{pair.Key}", $"cid:{pair.Value}");
                }

                var branding = GetAppBranding(appId);
                var subject = $"Confirm your {branding.DisplayName} account";

                var request = new SendEmailRequest
                {
                    To = new List<string> { authUser.Email },
                    Subject = subject,
                    Body = htmlBody,
                    InlineAttachments = htmlBody.Contains("cid:") ? inlineAttachments : new List<InlineAttachmentDto>()
                };

                return await SendEmailApiAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred preparing user confirmation email for {Email}", authUser.Email);
                return false;
            }
        }

        public async Task<bool> SendGeneralEmail(string to, string subject, string body)
        {
            var primaryTo = ExtractFirstRecipient(to);
            var request = new SendEmailRequest
            {
                To = new List<string> { primaryTo },
                Subject = subject,
                Body = body
            };

            return await SendEmailApiAsync(request);
        }

        private static string ExtractFirstRecipient(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            char[] delimiters = new[] { ';', ',', '\r', '\n', '\t', ' ' };
            var parts = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0].Trim() : string.Empty;
        }

        private async Task<bool> SendEmailApiAsync(SendEmailRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CommunicationService");
                var url = $"{_commConfig.BaseUrl.TrimEnd('/')}/api/comm/v1/Email/send";
                
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully via Communication Service.");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Communication API returned error: {StatusCode} - {Body}", (int)response.StatusCode, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Communication Service email endpoint.");
                return false;
            }
        }

        private async Task<string> GetEmailBody(AuthUser authUser, string role, string appId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] {
                new Claim("payload", JsonConvert.SerializeObject(authUser)),
                new Claim("role", role),
                new Claim("app", appId)
            };
            var token = new JwtSecurityToken(
                _jwtOptions.Value.Issuer,
                null,
                claims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );
            var jwtPayLoad = new JwtSecurityTokenHandler().WriteToken(token);
            var baseDir = AppContext.BaseDirectory;
            var strHtml = await File.ReadAllTextAsync(Path.Combine(baseDir, "Resources", "email.html"));
            
            var branding = GetAppBranding(appId);

            strHtml = strHtml.Replace("#username", authUser.Email);
            strHtml = strHtml.Replace("#confirmationlink", $"https://localhost:5001/api/auth/v1/User/confirmuserwithconfirmationlink?tokenPayload={jwtPayLoad}");
            strHtml = strHtml.Replace("#appname", branding.DisplayName);
            strHtml = strHtml.Replace("#apptagline", branding.Tagline);
            strHtml = strHtml.Replace("#brandcolor", branding.BrandColor);
            strHtml = strHtml.Replace("#accentcolor", branding.AccentColor);
            strHtml = strHtml.Replace("#applink", branding.AppUrl);
            strHtml = strHtml.Replace("#year", DateTime.UtcNow.Year.ToString());

            return strHtml;
        }

        private class AppBranding
        {
            public string DisplayName { get; set; } = string.Empty;
            public string Tagline { get; set; } = string.Empty;
            public string BrandColor { get; set; } = "#4f46e5";
            public string AccentColor { get; set; } = "#e0e7ff";
            public string AppUrl { get; set; } = "https://localhost:3000";
        }

        private AppBranding GetAppBranding(string appId)
        {
            return appId?.ToLowerInvariant() switch
            {
                "fish-tracker" or "fish-tracker-app" => new AppBranding
                {
                    DisplayName = "Fish Tracker",
                    Tagline = "Track your catches and navigate the waters with ease.",
                    BrandColor = "#0284c7",
                    AccentColor = "#f0f9ff",
                    AppUrl = "https://localhost:3002"
                },
                "rfe-glam-app" or "rfe-glam" => new AppBranding
                {
                    DisplayName = "RFE Glam",
                    Tagline = "Connecting Panchayat administrations, local employers, and laborers.",
                    BrandColor = "#059669",
                    AccentColor = "#ecfdf5",
                    AppUrl = "https://localhost:5173"
                },
                _ => new AppBranding
                {
                    DisplayName = "RFE Auth",
                    Tagline = "Secure, unified authentication for the RFE ecosystem.",
                    BrandColor = "#4f46e5",
                    AccentColor = "#e0e7ff",
                    AppUrl = "https://localhost:3001"
                }
            };
        }

        // Inner request contract DTOs matching the Communication API structure
        private class SendEmailRequest
        {
            public List<string> To { get; set; } = new();
            public string Subject { get; set; }
            public string Body { get; set; }
            public List<InlineAttachmentDto> InlineAttachments { get; set; } = new();
        }

        private class InlineAttachmentDto
        {
            public string ContentId { get; set; }
            public string FileName { get; set; }
            public string ContentBase64 { get; set; }
            public string ContentType { get; set; }
        }
    }
}