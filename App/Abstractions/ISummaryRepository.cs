using MyFinances.Api.DTOs;
using MyFinances.App.Queries.Interfaces;

namespace MyFinances.App.Abstractions
{
    public interface ISummaryRepository
    {
        Task<IEnumerable<AccountSummaryDto>> GetAccountBalancesAsync(Guid userId);
        Task<ExpensesAndIncomeAggregates> GetIncomeAndExpensesAsync(Guid userId, DateTime from, DateTime to);
    }
}
