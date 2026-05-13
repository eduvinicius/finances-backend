using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Abstractions
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<PagedResultBase<Category>> GetPaginatedByUserIdAsync(Guid userId, CategoryFilters filters);
        Task<Category?> GetByIdAsync(Guid id, Guid userId);
    }
}
