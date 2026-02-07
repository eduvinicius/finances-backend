using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class SummaryDto
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public IEnumerable<AccountSummaryDto> Accounts { get; set; } = [];
    }

    public class AccountSummaryDto
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public decimal Balance { get; set; }
    }
}
