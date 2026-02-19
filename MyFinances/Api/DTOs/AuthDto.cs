using System.ComponentModel.DataAnnotations;

namespace MyFinances.Api.DTOs
{
    public class RegisterDto : UserDto
    {
        [Required]
        [MinLength(6)]
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
