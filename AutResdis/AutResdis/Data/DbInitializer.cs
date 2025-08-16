using AutResdis.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutResdis.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if we already have users
            if (await context.Users.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Create sample users
            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = HashPassword("admin123"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 