using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.DTOs
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Логін обов'язковий")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        public string Password { get; set; } = string.Empty;
    }
}