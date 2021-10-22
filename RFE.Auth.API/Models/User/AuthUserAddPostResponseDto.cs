using Newtonsoft.Json;
using RFE.Auth.API.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    /// <summary>
    /// AuthUserAddPostRequestDto
    /// </summary>
    public class AuthUserAddPostResponseDto : BaseDto
    {
        /// <summary>
        /// Message
        /// </summary>
        /// <value></value>
        [JsonProperty(Order = 2)]
        public string Message { get; set; }
    }
}