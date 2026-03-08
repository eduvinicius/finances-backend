using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;

namespace MyFinances.App.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> CreateAsync(CategoryDto dto);
        Task<PagedResultBase<Category>> GetPaginatedAsync(CategoryFilters filters);
    }

}
