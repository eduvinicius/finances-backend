using MyFinances.Api.DTOs;

namespace MyFinances.App.Queries.Interfaces
{
    public interface ISummaryQuery
    {
        Task<SummaryDto> GetSummaryAsync(DateTime from, DateTime to);
    }
}
