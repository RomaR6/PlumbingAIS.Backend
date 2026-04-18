using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IConfiguration _configuration;

        public AuthService(IGenericRepository<User> userRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _configuration = configuration;
        }

        public async Task<User?> RegisterAsync(UserRegisterDto dto)
        {
            var users = await _userRepo.GetAllAsync();
            if (users.Any(u => u.Username == dto.Username)) return null;

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password), // Наш SHA256
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                RoleId = dto.RoleId,
                CreatedAt = DateTime.Now
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveAsync();
            return user;
        }

        public async Task<string?> LoginAsync(UserLoginDto dto)
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == dto.Username && u.PasswordHash == HashPassword(dto.Password));

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
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
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