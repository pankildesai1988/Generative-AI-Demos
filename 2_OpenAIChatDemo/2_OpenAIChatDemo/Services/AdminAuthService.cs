using _2_OpenAIChatDemo.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace _2_OpenAIChatDemo.Services
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly ChatDbContext _context;

        public AdminAuthService(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var hash = ComputeSha256Hash(password);
            return await _context.AdminUsers
                .AnyAsync(u => u.Username == username && u.PasswordHash == hash);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
