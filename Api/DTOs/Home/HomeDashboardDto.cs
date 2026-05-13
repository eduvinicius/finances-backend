namespace MyFinances.Api.DTOs.Home
{
    public class HomeDashboardDto
    {
        public decimal TotalBalance { get; set; }
        public MonthSummaryDto MonthSummary { get; set; } = null!;
        public IEnumerable<HomeTransactionDto> RecentTransactions { get; set; } = [];
        public IEnumerable<DaySpendingDto> WeeklySpending { get; set; } = [];
        public TodaySpendingDto TodaySpending { get; set; } = null!;
    }

    public class MonthSummaryDto
    {
        public decimal TotalSpent { get; set; }
        public decimal TotalIncome { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class HomeTransactionDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public string? CategoryName { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class DaySpendingDto
    {
        public DateTime Date { get; set; }
        public int DayOfWeek { get; set; }
        public decimal Total { get; set; }
    }

    public class TodaySpendingDto
    {
        public decimal Total { get; set; }
        public IEnumerable<HomeTransactionDto> Transactions { get; set; } = [];
    }
}
