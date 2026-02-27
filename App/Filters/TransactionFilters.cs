using MyFinances.Domain.Enums;

namespace MyFinances.App.Filters
{
    public class TransactionFilters: PaginationFilterBase
    {
        public List<string> AccountIds { get; set; } = [];
        public List<string> CategoryIds { get; set; } = [];
        public List<decimal> Type { get; set; } = [];
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
