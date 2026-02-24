using MyFinances.Domain.Entities;
using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class TransactionResponseDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Account Account { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
