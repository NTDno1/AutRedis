using AutResdis.Models;
using AutResdis.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutResdis.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IRedisService _redisService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IJwtService jwtService,
            IRedisService redisService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Validate user credentials
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                // Check if user is already logged in on this device
                var deviceId = request.DeviceId ?? GenerateDeviceId(request);
                var isAlreadyLoggedIn = await _redisService.IsUserLoggedInOnDeviceAsync(user.Id.ToString(), deviceId);

                if (isAlreadyLoggedIn)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User is already logged in on this device"
                    };
                }

                // Generate tokens
                var token = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiryTime = DateTime.UtcNow.AddMinutes(60); // 60 minutes

                // Create user session
                var userSession = new UserSession
                {
                    UserId = user.Id.ToString(),
                    Username = user.Username,
                    Token = token,
                    DeviceId = deviceId,
                    UserAgent = request.UserAgent ?? string.Empty,
                    IpAddress = request.IpAddress ?? string.Empty,
                    LoginTime = DateTime.UtcNow,
                    ExpiryTime = expiryTime,
                    IsActive = true
                };

                // Store session in Redis
                var sessionStored = await _redisService.StoreUserSessionAsync(userSession);
                if (!sessionStored)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Failed to create session"
                    };
                }

                // Update user's last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} logged in successfully from device {DeviceId}", user.Username, deviceId);

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiryTime,
                    Username = user.Username,
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                var result = await _redisService.RemoveUserSessionAsync(token);
                if (result)
                {
                    _logger.LogInformation("User logged out successfully");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        public async Task<bool> ValidateSessionAsync(string token, string? deviceId = null)
        {
            try
            {
                // First validate JWT token
                if (!_jwtService.ValidateToken(token))
                {
                    return false;
                }

                // Check if token exists in Redis
                var session = await _redisService.GetUserSessionAsync(token);
                if (session == null || !session.IsActive || session.ExpiryTime <= DateTime.UtcNow)
                {
                    return false;
                }

                // If deviceId is provided, validate it matches
                if (!string.IsNullOrEmpty(deviceId) && session.DeviceId != deviceId)
                {
                    _logger.LogWarning("Device ID mismatch for token. Expected: {Expected}, Actual: {Actual}", 
                        session.DeviceId, deviceId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session");
                return false;
            }
        }

        public async Task<bool> ForceLogoutOtherDevicesAsync(string userId, string currentDeviceId)
        {
            try
            {
                var result = await _redisService.InvalidateOtherSessionsAsync(userId, currentDeviceId);
                if (result)
                {
                    _logger.LogInformation("Force logged out other devices for user {UserId}", userId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force logging out other devices for user {UserId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<UserSession>> GetUserActiveSessionsAsync(string userId)
        {
            try
            {
                var sessions = await _redisService.GetUserSessionsAsync(userId);
                return sessions.Where(s => s.IsActive && s.ExpiryTime > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions for user {UserId}", userId);
                return Enumerable.Empty<UserSession>();
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashedPassword = Convert.ToBase64String(hashedBytes);
            return hashedPassword == passwordHash;
        }

        private string GenerateDeviceId(LoginRequest request)
        {
            var deviceInfo = $"{request.UserAgent ?? ""}{request.IpAddress ?? ""}";
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceInfo));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 