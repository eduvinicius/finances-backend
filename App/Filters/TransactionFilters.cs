namespace MyFinances.App.Filters
{
    public class TransactionFilters: PaginationFilterBase
    {
        public Guid? AccountId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
