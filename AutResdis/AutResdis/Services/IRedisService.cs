using AutResdis.Models;

namespace AutResdis.Services
{
    public interface IRedisService
    {
        Task<bool> StoreUserSessionAsync(UserSession session);
        Task<UserSession?> GetUserSessionAsync(string token);
        Task<bool> RemoveUserSessionAsync(string token);
        Task<bool> IsTokenValidAsync(string token);
        Task<bool> IsUserLoggedInOnDeviceAsync(string userId, string deviceId);
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(string userId);
        Task<bool> InvalidateOtherSessionsAsync(string userId, string currentDeviceId);
        Task<bool> UpdateSessionExpiryAsync(string token, DateTime newExpiry);
    }
} 