namespace RFE.Auth.Core.Models.Shared
{
    public class SQLServerDockerConfig
    {
        public string DBServer { get; set; } = "localhost";
        public int DBPort { get; set; } = 1443;
        public string DBUser { get; set; }
        public string DBPassword { get; set; }
    }
}