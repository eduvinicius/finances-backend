using System.ComponentModel.DataAnnotations;

namespace MyFinances.Api.DTOs
{
    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; } = null!;
    }

    public class GoogleAuthResponseDto
    {
        public string Token { get; set; } = null!;
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
