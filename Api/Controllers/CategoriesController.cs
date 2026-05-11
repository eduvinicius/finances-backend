using MyFinances.Api.DTOs;
using MyFinances.App.Filters;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController(ICategoryService categoryService)
        : ControllerBase
    {
        private readonly ICategoryService _categoryService = categoryService;

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        [HttpPost("getAll")]
        public async Task<IActionResult> GetPaginatedCategories([FromBody] CategoryFilters filters)
        {
            var paginatedCategories = await _categoryService.GetPaginatedAsync(filters);
            return Ok(paginatedCategories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryDto dto)
        {
            var category = await _categoryService.CreateAsync(dto);
            return Ok(category);
        }

    }

}
