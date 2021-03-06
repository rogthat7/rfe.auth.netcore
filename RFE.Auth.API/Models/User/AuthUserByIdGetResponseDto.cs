using System.Collections.Generic;
using Newtonsoft.Json;
using RFE.Auth.API.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    /// <summary>
    /// AuthUserByIdGetResponseDto
    /// </summary>
    public class AuthUserByIdGetResponseDto : BaseDto
    {
        /// <summary>
        /// Data
        /// </summary>
        /// <value></value>
        [JsonProperty(Order = 2)]
        public AuthUserByIdGetResponse Data { get; set; }
    }
}