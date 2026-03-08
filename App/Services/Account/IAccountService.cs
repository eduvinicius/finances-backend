using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;

namespace MyFinances.App.Services
{
    public interface IAccountService
    {
        Task<AccountResponseDto> CreateAsync(AccountDto dto);
        Task<IEnumerable<AccountResponseDto>> GetAllAsync();
        Task<PagedResultBase<AccountResponseDto>> GetPaginatedAsync(AccountFilters filters);
        Task<AccountResponseDto> GetByIdAsync(Guid id);
        Task DeactivateAsync(Guid id);
    }
}
