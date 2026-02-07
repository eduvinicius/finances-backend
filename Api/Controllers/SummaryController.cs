using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFinances.Api.DTOs;
using MyFinances.App.Queries.Summary.Interfaces;
using MyFinances.Domain.Entities;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SummaryController(ISummaryQuery summaryQuery) : ControllerBase
    {
        private readonly ISummaryQuery _summaryQuery = summaryQuery;

        [HttpGet]
        public async Task<ActionResult<SummaryDto>> Get([FromQuery] DateTime from, [FromQuery] DateTime to)
        {

            var result = await _summaryQuery.GetSummaryAsync(from, to);

            return Ok(result);
        }
    }
}
