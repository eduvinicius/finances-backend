using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class TransactionDto
    {
        public Guid AccountId { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
