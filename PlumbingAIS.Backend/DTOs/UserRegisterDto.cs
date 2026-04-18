using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.DTOs
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Логін обов'язковий")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Логін має бути від 3 до 20 символів")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Прізвище обов'язкове")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Роль обов'язкова")]
        public int RoleId { get; set; }
    }
}