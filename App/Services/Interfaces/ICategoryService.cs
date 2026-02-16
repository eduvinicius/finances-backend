using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> CreateAsync(CategoryDto dto);
        Task<IEnumerable<Category>> GetPaginatedAsync(CategoryFilters filters);
    }

}
