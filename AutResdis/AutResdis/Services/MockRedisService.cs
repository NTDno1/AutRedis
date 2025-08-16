//using AutResdis.Models;
//using Microsoft.Extensions.Options;
//using AutResdis.Configuration;

//namespace AutResdis.Services
//{
//    public class MockRedisService : IRedisService
//    {
//        private readonly Dictionary<string, UserSession> _sessions = new();
//        private readonly Dictionary<string, HashSet<string>> _userSessions = new();
//        private readonly Dictionary<string, UserSession> _deviceSessions = new();
//        private readonly ILogger<MockRedisService> _logger;
//        private readonly string _instanceName;
//        private readonly int _defaultExpiryMinutes;

//        public MockRedisService(IOptions<RedisSettings> redisSettings, ILogger<MockRedisService> logger)
//        {
//            _instanceName = redisSettings.Value.InstanceName;
//            _defaultExpiryMinutes = redisSettings.Value.DefaultExpiryMinutes;
//            _logger = logger;
//        }

//        public async Task<bool> StoreUserSessionAsync(UserSession session)
//        {
//            try
//            {
//                var sessionKey = $"{_instanceName}session:{session.Token}";
//                var userSessionsKey = $"{_instanceName}user:{session.UserId}:sessions";
//                var deviceKey = $"{_instanceName}user:{session.UserId}:device:{session.DeviceId}";

//                // Store session
//                _sessions[sessionKey] = session;
                
//                // Store device info
//                _deviceSessions[deviceKey] = session;
                
//                // Add to user's session list
//                if (!_userSessions.ContainsKey(userSessionsKey))
//                {
//                    _userSessions[userSessionsKey] = new HashSet<string>();
//                }
//                _userSessions[userSessionsKey].Add(session.Token);

//                _logger.LogInformation("Session stored successfully for user {UserId} on device {DeviceId}", session.UserId, session.DeviceId);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to store session for user {UserId} on device {DeviceId}", session.UserId, session.DeviceId);
//                return false;
//            }
//        }

//        public async Task<UserSession?> GetUserSessionAsync(string token)
//        {
//            try
//            {
//                var sessionKey = $"{_instanceName}session:{token}";
//                if (_sessions.TryGetValue(sessionKey, out var session))
//                {
//                    // Check if session is expired
//                    if (session.ExpiryTime > DateTime.UtcNow)
//                    {
//                        return session;
//                    }
//                    else
//                    {
//                        // Remove expired session
//                        _sessions.Remove(sessionKey);
//                    }
//                }
//                return null;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get session for token {Token}", token);
//                return null;
//            }
//        }

//        public async Task<bool> RemoveUserSessionAsync(string token)
//        {
//            try
//            {
//                var sessionKey = $"{_instanceName}session:{token}";
//                if (_sessions.TryGetValue(sessionKey, out var session))
//                {
//                    var deviceKey = $"{_instanceName}user:{session.UserId}:device:{session.DeviceId}";
//                    var userSessionsKey = $"{_instanceName}user:{session.UserId}:sessions";

//                    _sessions.Remove(sessionKey);
//                    _deviceSessions.Remove(deviceKey);
                    
//                    if (_userSessions.ContainsKey(userSessionsKey))
//                    {
//                        _userSessions[userSessionsKey].Remove(token);
//                    }

//                    _logger.LogInformation("Session removed successfully for user {UserId} on device {DeviceId}", session.UserId, session.DeviceId);
//                    return true;
//                }
//                return false;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to remove session for token {Token}", token);
//                return false;
//            }
//        }

//        public async Task<bool> IsUserLoggedInOnDeviceAsync(string userId, string deviceId)
//        {
//            try
//            {
//                var deviceKey = $"{_instanceName}user:{userId}:device:{deviceId}";
//                if (_deviceSessions.TryGetValue(deviceKey, out var session))
//                {
//                    return session.ExpiryTime > DateTime.UtcNow;
//                }
//                return false;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to check if user {UserId} is logged in on device {DeviceId}", userId, deviceId);
//                return false;
//            }
//        }

//        public async Task<List<UserSession>> GetUserActiveSessionsAsync(string userId)
//        {
//            try
//            {
//                var userSessionsKey = $"{_instanceName}user:{userId}:sessions";
//                var sessions = new List<UserSession>();

//                if (_userSessions.ContainsKey(userSessionsKey))
//                {
//                    foreach (var token in _userSessions[userSessionsKey])
//                    {
//                        var session = await GetUserSessionAsync(token);
//                        if (session != null)
//                        {
//                            sessions.Add(session);
//                        }
//                    }
//                }

//                return sessions;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get active sessions for user {UserId}", userId);
//                return new List<UserSession>();
//            }
//        }

//        public async Task<bool> RemoveAllUserSessionsExceptCurrentAsync(string userId, string currentToken)
//        {
//            try
//            {
//                var userSessionsKey = $"{_instanceName}user:{userId}:sessions";
//                if (_userSessions.ContainsKey(userSessionsKey))
//                {
//                    var tokensToRemove = _userSessions[userSessionsKey].Where(t => t != currentToken).ToList();
//                    foreach (var token in tokensToRemove)
//                    {
//                        await RemoveUserSessionAsync(token);
//                    }
//                    return true;
//                }
//                return false;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to remove all sessions except current for user {UserId}", userId);
//                return false;
//            }
//        }

//        public async Task<bool> IsTokenValidAsync(string token)
//        {
//            try
//            {
//                var session = await GetUserSessionAsync(token);
//                return session != null && session.IsActive && session.ExpiryTime > DateTime.UtcNow;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to validate token {Token}", token);
//                return false;
//            }
//        }

//        public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string userId)
//        {
//            try
//            {
//                var userSessionsKey = $"{_instanceName}user:{userId}:sessions";
//                var sessions = new List<UserSession>();

//                if (_userSessions.ContainsKey(userSessionsKey))
//                {
//                    foreach (var token in _userSessions[userSessionsKey])
//                    {
//                        var session = await GetUserSessionAsync(token);
//                        if (session != null)
//                        {
//                            sessions.Add(session);
//                        }
//                    }
//                }

//                return sessions;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get sessions for user {UserId}", userId);
//                return new List<UserSession>();
//            }
//        }

//        public async Task<bool> InvalidateOtherSessionsAsync(string userId, string currentDeviceId)
//        {
//            try
//            {
//                var userSessionsKey = $"{_instanceName}user:{userId}:sessions";
//                if (_userSessions.ContainsKey(userSessionsKey))
//                {
//                    var sessionsToInvalidate = new List<string>();
//                    foreach (var token in _userSessions[userSessionsKey])
//                    {
//                        if (_sessions.TryGetValue($"{_instanceName}session:{token}", out var session))
//                        {
//                            if (session.DeviceId != currentDeviceId)
//                            {
//                                sessionsToInvalidate.Add(token);
//                            }
//                        }
//                    }

//                    foreach (var token in sessionsToInvalidate)
//                    {
//                        await RemoveUserSessionAsync(token);
//                    }
//                    return true;
//                }
//                return false;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to invalidate other sessions for user {UserId}", userId);
//                return false;
//            }
//        }

//        public async Task<bool> UpdateSessionExpiryAsync(string token, DateTime newExpiry)
//        {
//            try
//            {
//                var sessionKey = $"{_instanceName}session:{token}";
//                if (_sessions.TryGetValue(sessionKey, out var session))
//                {
//                    session.ExpiryTime = newExpiry;
//                    _sessions[sessionKey] = session;
//                    return true;
//                }
//                return false;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to update session expiry for token {Token}", token);
//                return false;
//            }
//        }
//    }
//} 