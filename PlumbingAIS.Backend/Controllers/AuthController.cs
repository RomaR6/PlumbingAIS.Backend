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

        public AuthController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _authService.RegisterAsync(dto);
            if (user == null)
                return BadRequest(new { message = "Користувач із таким логіном вже існує" });

            var log = new ActionLog
            {
                Action = $"Нова реєстрація: {user.Username}",
                UserId = user.Id,
                Timestamp = DateTime.Now
            };
            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Реєстрація успішна" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.LoginAsync(dto);
            if (token == null)
                return Unauthorized(new { message = "Невірний логін або пароль" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user != null)
            {
                _context.ActionLogs.Add(new ActionLog
                {
                    Action = "Вхід у систему",
                    UserId = user.Id,
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return Ok(new { token });
        }

        [HttpGet("roles")]
        [AllowAnonymous]
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