using MyFinances.Api.DTOs;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Abstractions
{
    public interface ICategoryReportRepository
    {
        Task<List<CategoryReportDto>> GetCategoryReportAsync(Guid userId, DateTime from, DateTime to, TransactionType transactionType);
    }
}
