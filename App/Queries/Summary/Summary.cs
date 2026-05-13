using MyFinances.App.Abstractions;
using MyFinances.App.DTOs;
using MyFinances.App.Queries.Interfaces;
using MyFinances.App.Services;

namespace MyFinances.App.Queries.Summary
{
    public class SummaryQuery(
        ISummaryRepository summaryRepository,
        ICurrentUserService currentUserService
        ) : ISummaryQuery
    {
        private readonly ISummaryRepository _summaryRepository = summaryRepository;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<SummaryDto> GetSummaryAsync(DateTime from, DateTime to)
        {
            var userId = _currentUserService.UserId;
            var accountBalances = await _summaryRepository.GetAccountBalancesAsync(userId);
            var incomeAndExpenses = await _summaryRepository.GetIncomeAndExpensesAsync(userId, from, to);

            return new SummaryDto
            {
                TotalBalance = accountBalances.Sum(a => a.Balance),
                TotalIncome = incomeAndExpenses.Income,
                TotalExpenses = incomeAndExpenses.Expenses,
                Accounts = accountBalances
            };
        }
    }
}
