using Microsoft.EntityFrameworkCore;
using MyFinances.Api.DTOs.Home;
using MyFinances.App.Abstractions;
using MyFinances.Domain.Enums;
using MyFinances.Infrastructure.Data;

namespace MyFinances.App.Services.Home
{
    public class HomeService(
        FinanceDbContext context,
        ICurrentUserService currentUserService
    ) : IHomeService
    {
        private readonly FinanceDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<HomeDashboardDto> GetDashboardAsync(CancellationToken ct = default)
        {
            var userId = _currentUserService.UserId;
            var now = DateTime.UtcNow;

            var totalBalance = await GetTotalBalanceAsync(userId, ct);
            var monthSummary = await GetMonthSummaryAsync(userId, now, ct);
            var recentTransactions = await GetRecentTransactionsAsync(userId, ct);
            var weeklySpending = await GetWeeklySpendingAsync(userId, now, ct);
            var todaySpending = await GetTodaySpendingAsync(userId, now, ct);

            return new HomeDashboardDto
            {
                TotalBalance = totalBalance,
                MonthSummary = monthSummary,
                RecentTransactions = recentTransactions,
                WeeklySpending = weeklySpending,
                TodaySpending = todaySpending
            };
        }

        private async Task<decimal> GetTotalBalanceAsync(Guid userId, CancellationToken ct)
        {
            return await _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.IsActive)
                .SumAsync(a => a.Balance, ct);
        }

        private async Task<MonthSummaryDto> GetMonthSummaryAsync(Guid userId, DateTime now, CancellationToken ct)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var dayEnd = now.Date.AddDays(1).ToUniversalTime();

            var baseQuery = _context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.CreatedAt >= monthStart &&
                    t.CreatedAt < dayEnd);

            var totalSpent = await baseQuery
                .Where(t => t.Type == TransactionType.Expense)
                .SumAsync(t => Math.Abs(t.Amount), ct);

            var totalIncome = await baseQuery
                .Where(t => t.Type == TransactionType.Income)
                .SumAsync(t => t.Amount, ct);

            return new MonthSummaryDto
            {
                TotalSpent = totalSpent,
                TotalIncome = totalIncome,
                Month = now.Month,
                Year = now.Year
            };
        }

        private async Task<IEnumerable<HomeTransactionDto>> GetRecentTransactionsAsync(Guid userId, CancellationToken ct)
        {
            var transactions = await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Include(t => t.Account)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync(ct);

            return transactions.Select(MapToHomeTransactionDto);
        }

        private async Task<IEnumerable<DaySpendingDto>> GetWeeklySpendingAsync(Guid userId, DateTime now, CancellationToken ct)
        {
            // ISO week: Monday = 1, Sunday = 7
            var todayLocal = now.Date;
            var dayOfWeekIso = GetIsoDayOfWeek(todayLocal.DayOfWeek);
            var weekStart = todayLocal.AddDays(-(dayOfWeekIso - 1)); // Monday of current week
            var weekEnd = weekStart.AddDays(7);

            var weekStartUtc = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);
            var weekEndUtc = DateTime.SpecifyKind(weekEnd, DateTimeKind.Utc);

            var expensesByDay = await _context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransactionType.Expense &&
                    t.CreatedAt >= weekStartUtc &&
                    t.CreatedAt < weekEndUtc)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(t => Math.Abs(t.Amount))
                })
                .ToListAsync(ct);

            var lookup = expensesByDay.ToDictionary(e => e.Date, e => e.Total);

            return Enumerable.Range(0, 7).Select(offset =>
            {
                var date = weekStart.AddDays(offset);
                return new DaySpendingDto
                {
                    Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                    DayOfWeek = offset + 1, // Monday=1 … Sunday=7
                    Total = lookup.TryGetValue(date, out var total) ? total : 0m
                };
            });
        }

        private async Task<TodaySpendingDto> GetTodaySpendingAsync(Guid userId, DateTime now, CancellationToken ct)
        {
            var todayStart = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
            var todayEnd = todayStart.AddDays(1);

            var transactions = await _context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransactionType.Expense &&
                    t.CreatedAt >= todayStart &&
                    t.CreatedAt < todayEnd)
                .Include(t => t.Account)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);

            var total = transactions.Sum(t => Math.Abs(t.Amount));

            return new TodaySpendingDto
            {
                Total = total,
                Transactions = transactions.Select(MapToHomeTransactionDto)
            };
        }

        private static HomeTransactionDto MapToHomeTransactionDto(Transaction t) => new()
        {
            Id = t.Id,
            Description = t.Description,
            Amount = t.Amount,
            Type = (int)t.Type,
            CategoryName = t.Category?.Name,
            AccountName = t.Account.Name,
            CreatedAt = t.CreatedAt
        };

        private static int GetIsoDayOfWeek(DayOfWeek dow) => dow switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(dow))
        };
    }
}
