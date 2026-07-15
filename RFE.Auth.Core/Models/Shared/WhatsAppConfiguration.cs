namespace RFE.Auth.Core.Models.Shared
{
    /// <summary>
    /// WhatsApp Business Cloud API configuration.
    /// All values are read from appsettings.json → "WhatsAppConfiguration".
    /// Replace placeholders with real values from Meta Developer Portal.
    /// </summary>
    public class WhatsAppConfiguration
    {
        /// <summary>
        /// Meta Cloud API base URL.
        /// Default: https://graph.facebook.com/v19.0
        /// </summary>
        public string ApiBaseUrl { get; set; }

        /// <summary>
        /// Phone Number ID from Meta Developer Portal → WhatsApp → Getting Started.
        /// Example: "123456789012345"
        /// </summary>
        public string PhoneNumberId { get; set; }

        /// <summary>
        /// Permanent System User Access Token generated in Meta Business Suite.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Sender display name / number registered on Meta.
        /// </summary>
        public string FromPhoneNumber { get; set; }

        /// <summary>
        /// Default message template namespace (optional — only needed for template messages).
        /// </summary>
        public string TemplateNamespace { get; set; }

        /// <summary>
        /// Default language code for template messages (e.g. "en_US").
        /// </summary>
        public string TemplateLanguageCode { get; set; }
    }
}
