using MyFinances.App.Filters;
using MyFinances.Domain.Entities;

namespace MyFinances.Infrasctructure.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetPaginatedByUserIdAsync(Guid userId, CategoryFilters filters);
    }
}
