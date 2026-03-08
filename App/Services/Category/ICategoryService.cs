using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;

namespace MyFinances.App.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllAsync();
        Task<CategoryResponseDto> CreateAsync(CategoryDto dto);
        Task<PagedResultBase<CategoryResponseDto>> GetPaginatedAsync(CategoryFilters filters);
    }

}
