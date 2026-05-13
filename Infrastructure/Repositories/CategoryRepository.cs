using Microsoft.EntityFrameworkCore;
using MyFinances.App.Filters;
using MyFinances.Domain.Entities;
using MyFinances.Infrastructure.Data;
using MyFinances.App.Abstractions;
using MyFinances.App.Utils;
using MyFinances.App.Shared;

namespace MyFinances.Infrastructure.Repositories
{
    public class CategoryRepository(FinanceDbContext context) : Repository<Category>(context), ICategoryRepository
    {
        public override Task<Category?> GetByIdAsync(Guid id) =>
            throw new NotSupportedException("Use GetByIdAsync(Guid id, Guid userId) to ensure ownership scoping.");

        public async Task<Category?> GetByIdAsync(Guid id, Guid userId) =>
            await _context.Categories.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        public async Task<PagedResultBase<Category>> GetPaginatedByUserIdAsync(Guid userId, CategoryFilters filters)
        {
            var typeEnums = ConvertStringToArrayEnum.Convert(filters.Type);

            var query = _context.Categories
                .Where(c => c.UserId == userId);

            if (!string.IsNullOrEmpty(filters.Name))
                query = query.Where(c => c.Name.Contains(filters.Name));

            if (typeEnums.Count != 0)
                query = query.Where(c => typeEnums.Contains((int)c.Type));

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filters.ToDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return new PagedResultBase<Category>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
