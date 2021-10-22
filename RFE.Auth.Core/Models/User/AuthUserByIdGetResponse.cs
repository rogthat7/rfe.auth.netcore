using System.Text.Json.Serialization;

namespace RFE.Auth.Core.Models.User
{
    /// <summary>
    /// AuthUserByIdGetResponse
    /// </summary>
    public class AuthUserByIdGetResponse : AuthUser
    {
        /// <summary>
        /// Password
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public override string Password { get; set; }
    }
}