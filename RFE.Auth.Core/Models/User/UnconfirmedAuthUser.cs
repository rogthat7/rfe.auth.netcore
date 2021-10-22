using System;
using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
    [Serializable]
    public class UnconfirmedAuthUser : AuthUser
    {
        [JsonIgnore]
        public override bool Confirmed { get; set; } = false;
    }
}