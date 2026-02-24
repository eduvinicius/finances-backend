using MyFinances.Domain.Enums;

namespace MyFinances.App.Filters
{
    public class TransactionFilters: PaginationFilterBase
    {
        public List<string> AccountId { get; set; } = [];
        public List<string> CategoryId { get; set; } = [];
        public string Type { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
