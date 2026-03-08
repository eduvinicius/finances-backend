namespace MyFinances.Api.DTOs
{
    public class UserResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Nickname { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
