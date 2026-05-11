using System.ComponentModel.DataAnnotations;

namespace MyFinances.App.DTOs
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

    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; } = null!;
    }

    public class GoogleAuthResponseDto
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
        public GoogleUserDto User { get; set; } = null!;
    }

    public class GoogleUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? PictureUrl { get; set; }
    }
}
