using StackExchange.Redis;
using System.Text.Json;
using AutResdis.Models;
using Microsoft.Extensions.Options;
using AutResdis.Configuration;

namespace AutResdis.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly string _instanceName;
        private readonly int _defaultExpiryMinutes;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redis, IOptions<RedisSettings> redisSettings, ILogger<RedisService> logger)
        {
            _redis = redis;
            _instanceName = redisSettings.Value.InstanceName;
            _defaultExpiryMinutes = redisSettings.Value.DefaultExpiryMinutes;
            _logger = logger;
        }

        public async Task<bool> StoreUserSessionAsync(UserSession session)
        {
            try
            {
                var db = _redis.GetDatabase();
                var sessionKey = $"{_instanceName}session:{session.Token}";
                var userSessionsKey = $"{_instanceName}user:{session.UserId}:sessions";
                var deviceKey = $"{_instanceName}user:{session.UserId}:device:{session.DeviceId}";

                var sessionJson = JsonSerializer.Serialize(session);
                var expiry = TimeSpan.FromMinutes(_defaultExpiryMinutes);

                // Store session
                await db.StringSetAsync(sessionKey, sessionJson, expiry);
                
                // Store device info
                await db.StringSetAsync(deviceKey, sessionJson, expiry);
                
                // Add to user's session list
                await db.SetAddAsync(userSessionsKey, session.Token);
                await db.KeyExpireAsync(userSessionsKey, expiry);

                _logger.LogInformation("Session stored successfully for user {UserId} on device {DeviceId}", session.UserId, session.DeviceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store session for user {UserId} on device {DeviceId}", session.UserId, session.DeviceId);
                return false;
            }
        }

        public async Task<UserSession?> GetUserSessionAsync(string token)
        {
            try
            {
                var db = _redis.GetDatabase();
                var sessionKey = $"{_instanceName}session:{token}";
                var sessionJson = await db.StringGetAsync(sessionKey);

                if (sessionJson.HasValue)
                {
                    return JsonSerializer.Deserialize<UserSession>(sessionJson!);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get session for token {Token}", token);
                return null;
            }
        }

        public async Task<bool> RemoveUserSessionAsync(string token)
        {
            try
            {
                var db = _redis.GetDatabase();
                var sessionKey = $"{_instanceName}session:{token}";
                var session = await GetUserSessionAsync(token);

                if (session != null)
                {
                    var deviceKey = $"{_instanceName}user:{session.UserId}:device:{session.DeviceId}";
                    var userSessionsKey = $"{_instanceName}user:{session.UserId}:sessions";

                    await db.KeyDeleteAsync(sessionKey);
                    await db.KeyDeleteAsync(deviceKey);
                    await db.SetRemoveAsync(userSessionsKey, token);

                    _logger.LogInformation("Session removed successfully for user {UserId} on device {DeviceId}", session.UserId, session.DeviceId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove session for token {Token}", token);
                return false;
            }
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var session = await GetUserSessionAsync(token);
            return session != null && session.IsActive && session.ExpiryTime > DateTime.UtcNow;
        }

        public async Task<bool> IsUserLoggedInOnDeviceAsync(string userId, string deviceId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var deviceKey = $"{_instanceName}user:{userId}:device:{deviceId}";
                var deviceSession = await db.StringGetAsync(deviceKey);

                if (deviceSession.HasValue)
                {
                    var session = JsonSerializer.Deserialize<UserSession>(deviceSession!);
                    return session != null && session.IsActive && session.ExpiryTime > DateTime.UtcNow;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var userSessionsKey = $"{_instanceName}user:{userId}:sessions";
                var sessionTokens = await db.SetMembersAsync(userSessionsKey);

                var sessions = new List<UserSession>();
                foreach (var token in sessionTokens)
                {
                    var session = await GetUserSessionAsync(token!);
                    if (session != null)
                    {
                        sessions.Add(session);
                    }
                }

                return sessions;
            }
            catch
            {
                return Enumerable.Empty<UserSession>();
            }
        }

        public async Task<bool> InvalidateOtherSessionsAsync(string userId, string currentDeviceId)
        {
            try
            {
                var sessions = await GetUserSessionsAsync(userId);
                var otherSessions = sessions.Where(s => s.DeviceId != currentDeviceId);

                foreach (var session in otherSessions)
                {
                    await RemoveUserSessionAsync(session.Token);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSessionExpiryAsync(string token, DateTime newExpiry)
        {
            try
            {
                var session = await GetUserSessionAsync(token);
                if (session != null)
                {
                    session.ExpiryTime = newExpiry;
                    await StoreUserSessionAsync(session);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
} 