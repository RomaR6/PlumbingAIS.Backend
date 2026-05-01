using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Data;

namespace PlumbingAIS.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthService(IGenericRepository<User> userRepo, IConfiguration configuration, AppDbContext context)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _context = context;
        }

        public async Task<User?> RegisterAsync(UserRegisterDto dto)
        {
            var existingUsers = await _userRepo.GetAllAsync();
            if (existingUsers.Any(u => u.Username == dto.Username)) return null;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role)
                       ?? await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                RoleId = role?.Id ?? 2,
                CreatedAt = DateTime.Now
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveAsync();
            return user;
        }

        public async Task<string?> LoginAsync(UserLoginDto dto)
        {
            var hashedPassword = HashPassword(dto.Password);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == hashedPassword);

            if (user == null) return null;

            return CreateToken(user);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
                new Claim(ClaimTypes.Surname, user.LastName ?? "")
            };

            var tokenKey = _configuration.GetSection("AppSettings:Token").Value;
            if (string.IsNullOrEmpty(tokenKey))
            {
                throw new Exception("JWT Token key is missing in appsettings.json!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}