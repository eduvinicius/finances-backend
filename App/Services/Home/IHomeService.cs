using MyFinances.Api.DTOs.Home;

namespace MyFinances.App.Services.Home
{
    public interface IHomeService
    {
        Task<HomeDashboardDto> GetDashboardAsync(CancellationToken ct = default);
    }
}
