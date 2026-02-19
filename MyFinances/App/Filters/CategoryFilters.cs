namespace MyFinances.App.Filters
{
    public class CategoryFilters: PaginationFilterBase
    {
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
