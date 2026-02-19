using MyFinances.Api.DTOs;
using MyFinances.Api.Models;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDto dto);
        Task<UserResponse> LoginAsync(LoginDto dto);
        Task<string> UploadProfileImageAsync(Guid userId, IFormFile file);
        Task<User> UpdateUserAsync(UserDto user);
    }
}
