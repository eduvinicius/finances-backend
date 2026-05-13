using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Abstractions
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<PagedResultBase<Account>> GetPaginatedByUserIdAsync(Guid userId, AccountFilters filters);
        Task<Account?> GetByIdAsync(Guid id, Guid userId);
    }
}
