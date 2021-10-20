using Newtonsoft.Json;

namespace RFE.Auth.API.Models.Shared
{
    public class BaseDto
    {
        [JsonProperty(Order = 1)]
        public string Status { get; set; }
    }
}