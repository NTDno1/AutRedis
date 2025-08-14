using AutResdis.Models;

namespace AutResdis.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        string? GetUsernameFromToken(string token);
    }
} 