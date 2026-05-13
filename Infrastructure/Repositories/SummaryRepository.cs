using Microsoft.EntityFrameworkCore;
using MyFinances.Api.DTOs;
using MyFinances.App.Abstractions;
using MyFinances.App.Queries.Interfaces;
using MyFinances.Domain.Enums;
using MyFinances.Infrastructure.Data;

namespace MyFinances.Infrastructure.Repositories
{
    public class SummaryRepository(FinanceDbContext context) : ISummaryRepository
    {
        private readonly FinanceDbContext _context = context;

        public async Task<IEnumerable<AccountSummaryDto>> GetAccountBalancesAsync(Guid userId)
        {
            return await _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => new AccountSummaryDto
                {
                    AccountId = a.Id,
                    Name = a.Name,
                    Type = a.Type,
                    Balance = a.Balance
                })
                .ToListAsync();
        }

        public async Task<ExpensesAndIncomeAggregates> GetIncomeAndExpensesAsync(Guid userId, DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date.AddDays(1);

            var aggregates = await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.CreatedAt >= start && t.CreatedAt < end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => Math.Abs(t.Amount))
                })
                .FirstOrDefaultAsync();

            if (aggregates == null)
                return new ExpensesAndIncomeAggregates { Income = 0m, Expenses = 0m };

            return new ExpensesAndIncomeAggregates
            {
                Income = aggregates.Income,
                Expenses = aggregates.Expenses
            };
        }
    }
}
