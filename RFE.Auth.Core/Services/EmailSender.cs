using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Utils;
using Newtonsoft.Json;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly ILogger<EmailSender> _logger;
        private readonly string DEFAULT_USER_CONFIRMATION_SUBJECT = "DEFAULT_USER_CONFIRMATION_SUBJECT";

        public EmailSender(EmailConfiguration emailConfig, ILogger<EmailSender> logger, IOptions<JwtOptions> jwtOptions)
        {
            _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
            _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<bool> Send(MimeMessage emailMessage)
        {
            bool emailSent = true; 
            using (var client = new SmtpClient())
            {
                 try
                 {
                     // Use secure modern TLS protocols
                     client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                     // Use SecureSocketOptions.Auto to handle STARTTLS (port 587) and SSL (port 465) properly
                     await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.Auto);
                     client.AuthenticationMechanisms.Remove("XOAUTH2");
                     await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                     
                     await client.SendAsync(emailMessage);
                 }
                 catch(Exception e){
                     emailSent = false;
                     _logger.LogError(e, "Error sending email: {Message}", e.Message);
                 }
                 finally
                 {
                     await client.DisconnectAsync(quit:true);
                     client.Dispose();
                 }
                 return emailSent;
            }
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) {Text = message.Content};
            return emailMessage;
        }

        public async Task<bool> SendUserConfirmationEmail(AuthUser authUser, string role = "appUser", string appId = "rfe-auth")
        {
            var emailMessage = await GetUserConfirmationEmailMessage(authUser, role, appId);
            return await Send(emailMessage);
        }

        public async Task<MimeMessage> GetUserConfirmationEmailMessage(AuthUser authUser, string role, string appId)
        {
            var key = _jwtOptions.Value.JwtKeyForEmail;
            var htmlBody = await GetEmailBody(authUser, role, appId);
            var builder = new BodyBuilder();
            #region Processing Images ToDo
            var baseDir = AppContext.BaseDirectory;
            var image1 = builder.LinkedResources.Add(Path.Combine(baseDir, "Resources", "images", "image-1.png"));
            var image2 = builder.LinkedResources.Add(Path.Combine(baseDir, "Resources", "images", "image-2.png"));
            var image3 = builder.LinkedResources.Add(Path.Combine(baseDir, "Resources", "images", "image-3.png"));
            var image4 = builder.LinkedResources.Add(Path.Combine(baseDir, "Resources", "images", "image-4.png"));
            var image5 = builder.LinkedResources.Add(Path.Combine(baseDir, "Resources", "images", "image-5.png"));
            var image6 = builder.LinkedResources.Add(Path.Combine(baseDir, "Resources", "images", "image-6.png"));

            image1.ContentId = MimeUtils.GenerateMessageId ();
            image2.ContentId = MimeUtils.GenerateMessageId ();
            image3.ContentId = MimeUtils.GenerateMessageId ();
            image4.ContentId = MimeUtils.GenerateMessageId ();
            image5.ContentId = MimeUtils.GenerateMessageId ();
            image6.ContentId = MimeUtils.GenerateMessageId ();
            
            // Replaced without extra escaped double quotes so html reads standard src="cid:..."
            htmlBody = htmlBody.Replace("images/image-1.png", $"cid:{image1.ContentId}");
            htmlBody = htmlBody.Replace("images/image-2.png", $"cid:{image2.ContentId}");
            htmlBody = htmlBody.Replace("images/image-3.png", $"cid:{image3.ContentId}");
            htmlBody = htmlBody.Replace("images/image-4.png", $"cid:{image4.ContentId}");
            htmlBody = htmlBody.Replace("images/image-5.png", $"cid:{image5.ContentId}");
            htmlBody = htmlBody.Replace("images/image-6.png", $"cid:{image6.ContentId}");
            #endregion
            builder.HtmlBody = htmlBody; 
            Message message = new Message(new string[] {authUser.Email}, DEFAULT_USER_CONFIRMATION_SUBJECT, builder.HtmlBody ); 
            var emailMessage = CreateEmailMessage(message);
            return emailMessage;
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
                expires:DateTime.UtcNow.AddDays(1),// ToDo -- Change to 15 mins 
                signingCredentials:credentials
            );
            var jwtPayLoad = new JwtSecurityTokenHandler().WriteToken(token);
            var baseDir = AppContext.BaseDirectory;
            var strHtml = await File.ReadAllTextAsync(Path.Combine(baseDir, "Resources", "email.html"));
            strHtml = strHtml.Replace("#username", authUser.Email);
            strHtml = strHtml.Replace("#confirmationlink", $"https://localhost:5001/api/auth/v1/User/confirmuserwithconfirmationlink?tokenPayload={jwtPayLoad}");
            return strHtml;
        }
    }
}