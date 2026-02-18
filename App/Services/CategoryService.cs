using AutoMapper;
using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Services.Interfaces;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;
using MyFinances.Infrasctructure.Repositories.Interfaces;

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

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            var userId = _currentUserService.UserId;
            return await _categoryRepo.GetAllByUserIdAsync(userId);
        }

        public async Task<PagedResultBase<Category>> GetPaginatedAsync(CategoryFilters filters)
        {
            var userId = _currentUserService.UserId;
            return await _categoryRepo.GetPaginatedByUserIdAsync(userId, filters);
        }

        public async Task<Category> CreateAsync(CategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.UserId = _currentUserService.UserId;

            await _categoryRepo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category;
        }
    }
}
