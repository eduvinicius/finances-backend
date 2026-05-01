using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class TransactionExportDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? AccountId { get; set; }
        public TransactionType? Type { get; set; }
        public bool ExportAll { get; set; } = false;
    }
}
