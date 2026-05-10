using MyFinances.Api.DTOs;
using MyFinances.App.Shared;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Services.Admin
{
    public interface IAdminUserService
    {
        Task<PagedResultBase<AdminUserListItemDto>> GetUsersAsync(AdminUserFilterDto filters);
        Task<AdminUserDetailDto> GetUserByIdAsync(Guid userId);
        Task ChangeUserRoleAsync(Guid userId, UserRole newRole);
        Task DeactivateUserAsync(Guid userId);
        Task DeleteUserAsync(Guid userId);
    }
}
