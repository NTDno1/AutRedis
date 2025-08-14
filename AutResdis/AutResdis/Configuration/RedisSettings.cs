namespace AutResdis.Configuration
{
    public class RedisSettings
    {
        public string InstanceName { get; set; } = string.Empty;
        public int DefaultExpiryMinutes { get; set; }
    }
} 