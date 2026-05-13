using MyFinances.Api.DTOs.Home;
using MyFinances.App.Services.Home;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/home")]
    [Authorize]
    public class HomeController(IHomeService homeService) : ControllerBase
    {
        private readonly IHomeService _homeService = homeService;

        [HttpGet("dashboard")]
        public async Task<ActionResult<HomeDashboardDto>> GetDashboard(CancellationToken ct)
        {
            var dashboard = await _homeService.GetDashboardAsync(ct);
            return Ok(dashboard);
        }
    }
}
