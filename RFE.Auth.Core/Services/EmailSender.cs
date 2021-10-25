using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly IOptions<CustomOptions> _options;
        private readonly ILogger<EmailSender> _logger;
        private readonly string DEFAULT_USER_CONFIRMATION_SUBJECT = "DEFAULT_USER_CONFIRMATION_SUBJECT";
        private readonly string DEFAULT_USER_CONFIRMATION_BODY = "DEFAULT_USER_CONFIRMATION_BODY";

        public EmailSender(EmailConfiguration emailConfig, ILogger<EmailSender> logger, IOptions<CustomOptions> options)
        {
            _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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

        public async Task<bool> SendUserConfirmationEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            return await Send(emailMessage);
        }

        public async Task<Message> GetUserConfirmationEmailMessage(AuthUser authUser)
        {
            var myCustomOption = _options.Value.JwtKey;
            var emailBody = $"<i>Click here to Confirm your Registration<i></br>";
            Message message = new Message(new string[] {authUser.Email}, DEFAULT_USER_CONFIRMATION_SUBJECT,emailBody + DEFAULT_USER_CONFIRMATION_BODY ); 
            var emailMessage = CreateEmailMessage(message);
            return null;//await Send(emailMessage);
        }

    }
}