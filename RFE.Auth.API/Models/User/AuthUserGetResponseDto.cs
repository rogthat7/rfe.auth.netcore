using System.Collections.Generic;
using Newtonsoft.Json;
using RFE.Auth.API.Models.Shared;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.API.Models.User
{
    public class AuthUserGetResponseDto : BaseDto
    {
        [JsonProperty(Order = 2)]
        public List<AuthUserByIdGetResponse> Data { get; set; }
    }
}