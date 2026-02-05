namespace MyFinances.App.Filters
{
    public class PaginationFilterBase
    {
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }
}
