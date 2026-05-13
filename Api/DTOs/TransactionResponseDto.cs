using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public record TransactionAccountSummaryDto(Guid Id, string Name);
    public record TransactionCategorySummaryDto(Guid Id, string Name, string Type);

    public class TransactionResponseDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public TransactionAccountSummaryDto Account { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public TransactionCategorySummaryDto Category { get; set; } = null!;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
