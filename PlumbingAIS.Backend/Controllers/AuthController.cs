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
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var user = await _authService.RegisterAsync(dto);
            if (user == null) return BadRequest("Користувач вже існує");
            return Ok("Реєстрація успішна");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token == null) return BadRequest("Невірний логін або пароль");
            return Ok(new { token });
        }
    }
}