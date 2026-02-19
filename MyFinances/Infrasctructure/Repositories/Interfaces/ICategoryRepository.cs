using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.Infrasctructure.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<PagedResultBase<Category>> GetPaginatedByUserIdAsync(Guid userId, CategoryFilters filters);
    }
}
