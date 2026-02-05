using MyFinances.Api.DTOs;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface IAccountService
    {
        Task<Account> CreateAsync(AccountDto dto);
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account> GetByIdAsync(Guid id);
        Task DeactivateAsync(Guid id);
    }
}
