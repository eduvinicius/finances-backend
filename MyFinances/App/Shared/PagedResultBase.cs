namespace MyFinances.App.Shared
{
    public class PagedResultBase<T>
    {
        public IReadOnlyCollection<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
    }
}
