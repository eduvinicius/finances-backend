using Microsoft.EntityFrameworkCore;
using MyFinances.Api.DTOs;
using MyFinances.App.Queries.Interfaces;
using MyFinances.App.Services.Interfaces;
using MyFinances.Domain.Enums;
using MyFinances.Infrasctructure.Data;

namespace MyFinances.App.Queries.Summary
{
    public class SummaryQuery(
        FinanceDbContext context,
        ICurrentUserService currentUserService
        ) : ISummaryQuery
    {
        private readonly FinanceDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<SummaryDto> GetSummaryAsync(DateTime from, DateTime to)
        {
            var userId = _currentUserService.UserId;
            var accountBalances = await GetTotalBalance(userId);
            var incomeAndExpenses = await GetIncomeAndExpensesAsync(userId, from, to);

            return new SummaryDto
            {
                TotalBalance = accountBalances.Sum(a => a.Balance),
                TotalIncome = incomeAndExpenses.Income,
                TotalExpenses = incomeAndExpenses.Expenses,
                Accounts = accountBalances
            };
        }

        private async Task<IEnumerable<AccountSummaryDto>> GetTotalBalance(Guid userId)
        {
            var accountBalances = await _context.Accounts
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Select(a => new AccountSummaryDto
                {
                    AccountId = a.Id,
                    Name = a.Name,
                    Type = a.Type,
                    Balance = a.Balance
                })
                .ToListAsync();

            return accountBalances;
        }

        private async Task<ExpensesAndIncomeAggregates> GetIncomeAndExpensesAsync(Guid userId, DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date.AddDays(1);

            var aggregates = await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => Math.Abs(t.Amount))
                })
                .FirstOrDefaultAsync();

            if (aggregates == null)
            {
                return new ExpensesAndIncomeAggregates
                {
                    Income = 0m,
                    Expenses = 0m
                };
            }

            return new ExpensesAndIncomeAggregates
            {
                Income = aggregates.Income,
                Expenses = aggregates.Expenses
            };
        }
    }
}
