using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

            return Ok(new { token });
        }

        [HttpGet("roles")]
        [AllowAnonymous]
        public IActionResult GetAvailableRoles()
        {
            
            var roles = new[]
            {
                new { Id = 1, Name = "Admin", Description = "Повний доступ до управління складом та користувачами" },
                new { Id = 2, Name = "User", Description = "Доступ до перегляду товарів та створення транзакцій" }
            };

            return Ok(roles);
        }
    }
}