using Microsoft.AspNetCore.Mvc;
using AutResdis.Models;
using AutResdis.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutResdis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRegistrationRequest request)
        {
            try
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    return BadRequest("Username already exists");
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest("Email already exists");
                }

                // Hash password
                var passwordHash = HashPassword(request.Password);

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} registered successfully", user.Username);

                // Return user without password hash
                return Ok(new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.CreatedAt,
                    user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Username}", request.Username);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("profile")]
        public async Task<ActionResult> GetProfile()
        {
            try
            {
                // Get user ID from JWT token (you'll need to implement JWT authentication)
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .Where(u => u.Username == username)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.CreatedAt,
                        u.LastLoginAt,
                        u.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "Internal server error");
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public class UserRegistrationRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
} 