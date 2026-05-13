using MyFinances.Api.DTOs;
using MyFinances.App.Filters;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionController(ITransactionService transactionService) : ControllerBase
    {
        private readonly ITransactionService _transactionService = transactionService;

        [HttpPost("getAll")]
        public async Task<IActionResult> GetTransactions([FromBody] TransactionFilters filters)
        {
            var transactions = await _transactionService.GetAllByUserId(filters);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
        {
            var transaction = await _transactionService.CreateAsync(dto);
            return Ok(transaction);
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] TransactionExportDto dto)
        {
            var fileBytes = await _transactionService.ExportToExcelAsync(dto);
            var fileName = $"transactions_{DateTime.UtcNow:yyyyMMdd}.xlsx";
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
