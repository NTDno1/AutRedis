using AutResdis.Models;

namespace AutResdis.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync(string token);
        Task<bool> ValidateSessionAsync(string token, string? deviceId = null);
        Task<bool> ForceLogoutOtherDevicesAsync(string userId, string currentDeviceId);
        Task<IEnumerable<UserSession>> GetUserActiveSessionsAsync(string userId);
    }
} 