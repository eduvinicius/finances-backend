using MyFinances.Api.DTOs;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Abstractions
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGoogleSubjectIdAsync(string googleSubjectId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<PagedResultBase<User>> GetAllFilteredAsync(AdminUserFilterDto filters, Guid excludeUserId);
    }
}
