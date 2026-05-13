using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFinances.App.DTOs;
using MyFinances.App.Queries.Interfaces;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SummaryController(ISummaryQuery summaryQuery) : ControllerBase
    {
        private readonly ISummaryQuery _summaryQuery = summaryQuery;

        [HttpGet]
        public async Task<ActionResult<SummaryDto>> GetSummary([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _summaryQuery.GetSummaryAsync(from, to);

            return Ok(result);
        }
    }
}
