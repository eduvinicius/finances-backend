using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFinances.Api.DTOs;
using MyFinances.App.Queries.Interfaces;
using MyFinances.Domain.Entities;
using MyFinances.Domain.Enums;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SummaryController(
        ISummaryQuery summaryQuery,
        ICategoryReport categoryReport
        ) : ControllerBase
    {
        private readonly ISummaryQuery _summaryQuery = summaryQuery;
        private readonly ICategoryReport _categoryReport = categoryReport;

        [HttpGet("summary")]
        public async Task<ActionResult<SummaryDto>> GetSummary([FromQuery] DateTime from, [FromQuery] DateTime to)
        {

            var result = await _summaryQuery.GetSummaryAsync(from, to);

            return Ok(result);
        }

        [HttpGet("category")]
        public async Task<ActionResult<CategoryDto>> GetCategoryReport([FromQuery] DateTime from, DateTime to, TransactionType transactionType)
        {
            var result = await _categoryReport.GetCategoryReportAsync(from, to, transactionType);

            return Ok(result);
        }
    }
}
