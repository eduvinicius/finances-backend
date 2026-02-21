using MyFinances.Api.DTOs;
using MyFinances.Api.Models;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<string> UploadProfileImageAsync(Guid userId, IFormFile file);
        Task<User> UpdateUserAsync(Guid id, UpdateUserDto user);
        Task<Stream> GetProfileImageAsync(Guid userId);
    }
}
