using MyFinances.Domain.Enums;

namespace MyFinances.App.Filters
{
    public class CategoryFilters: PaginationFilterBase
    {
        public string Name { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
