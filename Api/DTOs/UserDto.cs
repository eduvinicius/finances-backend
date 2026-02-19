using System.ComponentModel.DataAnnotations;

namespace MyFinances.Api.DTOs
{
    public class UserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        public string Nickname { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }
        [Required]
        public string DocumentNumber { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        [Required] 
        public string Country { get; set; } = null!;
        [Required]
        public DateTime BirthDate { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
