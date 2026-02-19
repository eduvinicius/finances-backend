using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface IAccountService
    {
        Task<Account> CreateAsync(AccountDto dto);
        Task<IEnumerable<Account>> GetAllAsync();
        Task<PagedResultBase<Account>> GetPaginatedAsync(AccountFilters filters);
        Task<Account> GetByIdAsync(Guid id);
        Task DeactivateAsync(Guid id);
    }
}
