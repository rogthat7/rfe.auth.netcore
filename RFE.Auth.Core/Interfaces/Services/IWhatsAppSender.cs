using System.Threading.Tasks;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IWhatsAppSender
    {
        /// <summary>
        /// Sends a plain text WhatsApp message to the given phone number.
        /// </summary>
        /// <param name="toPhoneNumber">Recipient in E.164 format (e.g. "+919876543210").</param>
        /// <param name="message">Plain text message body.</param>
        Task<bool> SendTextMessage(string toPhoneNumber, string message);

        /// <summary>
        /// Sends a pre-approved WhatsApp template message.
        /// </summary>
        /// <param name="toPhoneNumber">Recipient in E.164 format.</param>
        /// <param name="templateName">Name of the approved template (e.g. "hello_world").</param>
        /// <param name="languageCode">BCP-47 language code (e.g. "en_US").</param>
        Task<bool> SendTemplateMessage(string toPhoneNumber, string templateName, string languageCode = null);
    }
}
