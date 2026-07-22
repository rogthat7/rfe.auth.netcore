namespace RFE.Auth.Core.Models.Shared
{
    public class PostgresDockerConfig
    {
        public string PostgresServer { get; set; } = "localhost";
        public int PostgresPort { get; set; } = 5432;
        public string PostgresUser { get; set; }
        public string PostgresPassword { get; set; }
    }
}
