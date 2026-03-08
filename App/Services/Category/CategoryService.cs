using AutoMapper;
using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;
using MyFinances.Infrastructure.Repositories.Interfaces;

namespace MyFinances.App.Services
{
    public class CategoryService(
        ICategoryRepository categoryRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper
    ): ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo = categoryRepo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
        {
            var userId = _currentUserService.UserId;
            var categories = await _categoryRepo.GetAllByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
        }

        public async Task<PagedResultBase<CategoryResponseDto>> GetPaginatedAsync(CategoryFilters filters)
        {
            var userId = _currentUserService.UserId;
            var result = await _categoryRepo.GetPaginatedByUserIdAsync(userId, filters);
            var mappedCategories = _mapper.Map<IEnumerable<CategoryResponseDto>>(result.Items);
            
            return new PagedResultBase<CategoryResponseDto>
            {
                Items = (IReadOnlyCollection<CategoryResponseDto>)mappedCategories,
                TotalCount = result.TotalCount
            };
        }

        public async Task<CategoryResponseDto> CreateAsync(CategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.UserId = _currentUserService.UserId;

            await _categoryRepo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryResponseDto>(category);
        }
    }
}
