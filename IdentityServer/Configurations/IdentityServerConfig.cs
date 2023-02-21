namespace IdentityServer.Configurations
{
    public class IdentityServerConfig
    {
        public string AuthConnectionString { get; set; }

        public string CottageConnectionString { get; set; }

        public string CertFullName { get; set; }

        public string CertPassword { get; set; }

        public string MinimumLogLevel { get; set; }

        public int CacheTimeInMinutes { get; set; }
    }
}
