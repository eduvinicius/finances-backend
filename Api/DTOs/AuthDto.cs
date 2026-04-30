using System.ComponentModel.DataAnnotations;

namespace MyFinances.Api.DTOs
{
    public class RegisterDto : UserDto
    {
        [Required]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
