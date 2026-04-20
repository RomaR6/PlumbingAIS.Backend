using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.DTOs
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Логін обов'язковий")]
        [StringLength(20, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public string FirstName { get; set; } = "Employee";

        public string LastName { get; set; } = "AIS";

        public string Role { get; set; } = "User";
    }
}