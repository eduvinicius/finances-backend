using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Services.Interfaces;

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

            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
        {
            var transaction = await _transactionService.CreateAsync(dto);
            return Ok(transaction);
        }
    }
}
