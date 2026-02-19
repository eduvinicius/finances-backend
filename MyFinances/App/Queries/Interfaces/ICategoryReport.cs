using MyFinances.Api.DTOs;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Queries.Interfaces
{
    public interface ICategoryReport
    {
        public Task<IEnumerable<CategoryReportDto>> GetCategoryReportAsync(DateTime from, DateTime to, TransactionType transactionType);
    }
}
