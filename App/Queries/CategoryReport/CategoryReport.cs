using MyFinances.App.Abstractions;
using MyFinances.App.DTOs;
using MyFinances.App.Queries.Interfaces;
using MyFinances.App.Services;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Queries.CategoryReport
{
    public class CategoryReport(
        ICategoryReportRepository categoryReportRepository,
        ICurrentUserService currentUserService
        ) : ICategoryReport
    {
        private readonly ICategoryReportRepository _categoryReportRepository = categoryReportRepository;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<IEnumerable<CategoryReportDto>> GetCategoryReportAsync(DateTime from, DateTime to, TransactionType transactionType)
        {
            var userId = _currentUserService.UserId;

            var categoriesReport = await _categoryReportRepository.GetCategoryReportAsync(userId, from, to, transactionType);

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
