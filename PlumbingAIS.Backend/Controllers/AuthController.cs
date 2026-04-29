using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.Data;
using System.Security.Claims;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private readonly ILoggerService _logger;

        public AuthController(IAuthService authService, AppDbContext context, ILoggerService logger)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authService.RegisterAsync(dto);
            if (user == null) return BadRequest(new { message = "Користувач із таким логіном вже існує" });

            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int adminId = adminIdStr != null ? int.Parse(adminIdStr) : 0;

            await _logger.LogActionAsync($"Адмін створив акаунт: {user.Username} (Роль: {user.RoleName})", adminId);
            return Ok(new { message = "Користувача успішно зареєстровано" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var token = await _authService.LoginAsync(dto);
            if (token == null) return Unauthorized(new { message = "Невірний логін або пароль" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user != null)
            {
                await _logger.LogActionAsync("Вхід у систему", user.Id);
            }

            return Ok(new { token });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userIdStr));

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.FirstName,
                user.LastName,
                user.CreatedAt,
                Role = user.RoleName
            });
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAvailableRoles()
        {
            var roles = new[]
            {
                new { Id = 1, Name = "Admin", Description = "Повний доступ до управління складом та користувачами" },
                new { Id = 2, Name = "Manager", Description = "Управління товарами та складом" },
                new { Id = 3, Name = "User", Description = "Перегляд товарів та створення транзакцій" }
            };
            return Ok(roles);
        }
    }
}