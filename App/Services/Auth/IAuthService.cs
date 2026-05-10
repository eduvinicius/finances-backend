using MyFinances.Api.DTOs;

namespace MyFinances.App.Services
{
    public interface IAuthService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<GoogleAuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
        Task<string> UploadProfileImageAsync(Guid userId, IFormFile file);
        Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto user);
        Task<Stream> GetProfileImageAsync(Guid userId);
    }
}
