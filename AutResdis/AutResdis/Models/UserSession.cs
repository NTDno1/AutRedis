namespace AutResdis.Models
{
    public class UserSession
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 