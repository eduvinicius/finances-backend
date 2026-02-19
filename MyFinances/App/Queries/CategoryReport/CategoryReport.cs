using Microsoft.EntityFrameworkCore;
using MyFinances.Api.DTOs;
using MyFinances.App.Queries.Interfaces;
using MyFinances.App.Services.Interfaces;
using MyFinances.Domain.Enums;
using MyFinances.Infrasctructure.Data;

namespace MyFinances.App.Queries.CategoryReport
{
    public class CategoryReport(
        FinanceDbContext context,
        ICurrentUserService currentUserService
        ): ICategoryReport
    {

            private readonly FinanceDbContext _context = context;
            private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<IEnumerable<CategoryReportDto>> GetCategoryReportAsync(DateTime from, DateTime to, TransactionType transactionType)
        {
            var userId = _currentUserService.UserId;

            var start = from.Date;
            var end = to.Date.AddDays(1);

            var categoriesReport = await _context.Transactions
                .AsNoTracking()
                .Where(t => 
                    t.UserId == userId && 
                    t.Date >= start && 
                    t.Date <= end && 
                    t.Type == transactionType
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

            var total = categoriesReport.Sum(c => Math.Abs(c.TotalAmount));

            if (total == 0)
                return categoriesReport;

            foreach (var category in categoriesReport)
            {
                category.PercentageOfTotal = Math.Abs(category.TotalAmount) / total * 100;
            }

            return categoriesReport;
        }

    }
}
