namespace MyFinances.Api.Models
{
    public class UserResponse
    {
        public string FullName { get; set; } = null!;
        public string? NickName { get; set; }
        public string Token { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;

    }
}
