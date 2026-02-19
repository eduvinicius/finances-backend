using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class CategoryReportDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }
}
