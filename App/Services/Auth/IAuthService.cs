using MyFinances.Api.DTOs;

namespace MyFinances.App.Services
{
    public interface IAuthService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<string> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<GoogleAuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
        Task<string> UploadProfileImageAsync(Guid userId, IFormFile file);
        Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto user);
        Task<Stream> GetProfileImageAsync(Guid userId);
    }
}
