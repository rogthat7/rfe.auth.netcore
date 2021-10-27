using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
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
        private readonly string DEFAULT_USER_CONFIRMATION_BODY = "DEFAULT_USER_CONFIRMATION_BODY";

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
                     // Allow SSLv3.0 and all versions of TLS
                    client.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
                    // client.CheckCertificateRevocation  = false;
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, false);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    
                    await client.SendAsync(emailMessage);
                 }
                 catch(Exception e){
                     emailSent = false;
                     _logger.LogError(e.Message);
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

        public async Task<bool> SendUserConfirmationEmail(AuthUser authUser)
        {
            var emailMessage = await  GetUserConfirmationEmailMessage(authUser);
            return await Send(emailMessage);
        }

        public async Task<MimeMessage> GetUserConfirmationEmailMessage(AuthUser authUser)
        {
            var key = _jwtOptions.Value.JwtKeyForEmail;

            var emailBody = await GetEmailBody(authUser);
            Message message = new Message(new string[] {authUser.Email}, DEFAULT_USER_CONFIRMATION_SUBJECT,emailBody + DEFAULT_USER_CONFIRMATION_BODY ); 
            var emailMessage = CreateEmailMessage(message);
            return null;//await Send(emailMessage);
        }

        private async Task<string> GetEmailBody(AuthUser authUser)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtKeyForEmail));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[] {
                new Claim("payload",JsonConvert.SerializeObject(authUser)),
                
            };
            var token = new JwtSecurityToken(
                _jwtOptions.Value.Issuer,
                null,
                claims,
                DateTime.UtcNow,
                expires:DateTime.UtcNow.AddMinutes(15),
                signingCredentials:credentials
            );
            var jwtPayLoad = new JwtSecurityTokenHandler().WriteToken(token);
            var strHtml = await ReadAllTextAsync("./Resources/email.html");
            return strHtml;
            
        }
        public async Task<string> ReadAllTextAsync(string filePath)
        {
            int DefaultBufferSize = 4096;
            FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            DefaultBufferSize, DefaultOptions))
            {
                var sb = new StringBuilder();
                var buffer = new byte[0x1000];
                var numRead = 0;

                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    sb.Append(Encoding.Unicode.GetString(buffer, 0, numRead));
                return sb.ToString();
            }
        }
    }
}