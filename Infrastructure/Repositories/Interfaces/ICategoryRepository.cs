using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<PagedResultBase<Category>> GetPaginatedByUserIdAsync(Guid userId, CategoryFilters filters);
    }
}
