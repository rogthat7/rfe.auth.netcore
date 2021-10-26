namespace RFE.Auth.Core.Models.Shared
{
    public class JwtOptions
    {
        public string Secret {get; set;}
        public string Issuer {get; set;}
        public string JwtKeyForEmail {get; set;}
    }
}