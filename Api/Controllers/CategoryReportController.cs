using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFinances.App.DTOs;
using MyFinances.App.Queries.Interfaces;
using MyFinances.Domain.Enums;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryReportController(ICategoryReport categoryReport) : ControllerBase
    {
        private readonly ICategoryReport _categoryReport = categoryReport;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReportDto>>> GetCategoryReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] TransactionType transactionType)
        {
            var result = await _categoryReport.GetCategoryReportAsync(from, to, transactionType);

            return Ok(result);
        }
    }
}
