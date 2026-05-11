using Microsoft.EntityFrameworkCore;
using MyFinances.Api.DTOs;
using MyFinances.App.Abstractions;
using MyFinances.Domain.Enums;
using MyFinances.Infrastructure.Data;

namespace MyFinances.Infrastructure.Repositories
{
    public class CategoryReportRepository(FinanceDbContext context) : ICategoryReportRepository
    {
        private readonly FinanceDbContext _context = context;

        public async Task<List<CategoryReportDto>> GetCategoryReportAsync(
            Guid userId,
            DateTime from,
            DateTime to,
            TransactionType transactionType)
        {
            var start = from.Date;
            var end = to.Date.AddDays(1);

            var categoriesReport = await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t =>
                    t.UserId == userId &&
                    t.CreatedAt >= start &&
                    t.CreatedAt <= end &&
                    (transactionType == TransactionType.All || t.Type == transactionType)
                )
                .GroupBy(t => new
                {
                    t.CategoryId,
                    t.Category.Name
                })
                .Select(g => new CategoryReportDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            return categoriesReport;
        }
    }
}
