using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> CreateAsync(CategoryDto dto);
        Task<PagedResultBase<Category>> GetPaginatedAsync(CategoryFilters filters);
    }

}
