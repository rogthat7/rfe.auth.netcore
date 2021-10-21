using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
    public class UnconfirmedAuthUser : AuthUser
    {
        [JsonIgnore]
        public override bool Confirmed { get; set; } = false;
    }
}