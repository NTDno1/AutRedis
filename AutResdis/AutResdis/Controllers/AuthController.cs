using Microsoft.AspNetCore.Mvc;
using AutResdis.Models;
using AutResdis.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutResdis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Extract device information from request
                request.UserAgent = Request.Headers["User-Agent"].ToString();
                request.IpAddress = GetClientIpAddress();

                var response = await _authService.LoginAsync(request);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login endpoint");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var token = GetTokenFromHeader();
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token not found");
                }

                var result = await _authService.LogoutAsync(token);
                if (result)
                {
                    return Ok(new { Message = "Logout successful" });
                }

                return BadRequest("Logout failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in logout endpoint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("logout-other-devices")]
        [Authorize]
        public async Task<ActionResult> LogoutOtherDevices()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var token = GetTokenFromHeader();
                
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return BadRequest("Invalid request");
                }

                // Get current device ID from token
                var currentDeviceId = GetDeviceIdFromToken(token);
                if (string.IsNullOrEmpty(currentDeviceId))
                {
                    return BadRequest("Unable to identify current device");
                }

                var result = await _authService.ForceLogoutOtherDevicesAsync(userId, currentDeviceId);
                if (result)
                {
                    return Ok(new { Message = "Other devices logged out successfully" });
                }

                return BadRequest("Failed to logout other devices");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in logout other devices endpoint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("validate-session")]
        [Authorize]
        public async Task<ActionResult> ValidateSession()
        {
            try
            {
                var token = GetTokenFromHeader();
                var deviceId = GetDeviceIdFromToken(token ?? string.Empty);

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token not found");
                }

                var isValid = await _authService.ValidateSessionAsync(token, deviceId);
                if (isValid)
                {
                    return Ok(new { Valid = true, Message = "Session is valid" });
                }

                return Unauthorized(new { Valid = false, Message = "Session is invalid or expired" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in validate session endpoint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("active-sessions")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserSession>>> GetActiveSessions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID not found");
                }

                var sessions = await _authService.GetUserActiveSessionsAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get active sessions endpoint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        private string? GetTokenFromHeader()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            if (authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length);
            }
            return null;
        }

        private string? GetDeviceIdFromToken(string token)
        {
            // This is a simplified approach. In a real implementation,
            // you might want to store device ID in the JWT claims or
            // extract it from the Redis session
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = GetClientIpAddress();
            var deviceInfo = $"{userAgent}{ipAddress}";
            
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceInfo));
            return Convert.ToBase64String(hashedBytes);
        }

        private string GetClientIpAddress()
        {
            var forwardedHeader = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            return remoteIpAddress?.ToString() ?? "Unknown";
        }
    }
} 