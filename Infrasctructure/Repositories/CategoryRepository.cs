using Microsoft.EntityFrameworkCore;
using MyFinances.App.Filters;
using MyFinances.Domain.Entities;
using MyFinances.Infrasctructure.Data;
using MyFinances.Infrasctructure.Repositories.Interfaces;

namespace MyFinances.Infrasctructure.Repositories
{
    public class CategoryRepository(FinanceDbContext context) : Repository<Category>(context), ICategoryRepository
    {
        public async Task<IEnumerable<Category>> GetPaginatedByUserIdAsync(Guid userId, CategoryFilters filters)
        {
            var query = _context.Categories
                .Where(c => c.UserId == userId);
                

            if (!string.IsNullOrEmpty(filters.Name))
                query = query.Where(c => c.Name.Contains(filters.Name));

            if (filters.Type != default)
                query = query.Where(c => c.Type == filters.Type);


            if (filters.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filters.ToDate.Value);

            return await query
                .AsNoTracking()
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();
        }
    }
}
