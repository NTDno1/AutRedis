using System.ComponentModel.DataAnnotations;

namespace AutResdis.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        public string? DeviceId { get; set; }
        
        public string? UserAgent { get; set; }
        
        public string? IpAddress { get; set; }
    }
} 