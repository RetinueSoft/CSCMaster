namespace CSCMasterAPI.Utils
{
    /// <summary>
    /// JWT signing key from configuration ("Jwt:Key", or environment variable Jwt__Key).
    /// It must be secret: anyone with the key can create valid login tokens. At least 32 characters.
    /// </summary>
    public static class JwtSettings
    {
        // Only used when running in Development without a configured key (it is public in the git history).
        private const string DevelopmentKey = "M0MjGA3c4w6FhXpavzFwOuDchrBo9JSZ";

        public static string GetKey(IConfiguration config, IHostEnvironment? env = null)
        {
            var key = config["Jwt:Key"];
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (key.Length < 32)
                    throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
                return key;
            }
            var environment = env?.EnvironmentName ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
                return DevelopmentKey;
            throw new InvalidOperationException("Jwt:Key is not configured (set the Jwt__Key environment variable).");
        }
    }
}
