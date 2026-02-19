using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.Infrasctructure.Repositories.Interfaces
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<PagedResultBase<Account>> GetPaginatedByUserIdAsync(Guid userId, AccountFilters filters);
    }
}
