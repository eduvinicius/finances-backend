using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Services;
using MyFinances.App.Services.Interfaces;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize]
    public class AccountsController(IAccountService accountService)
        : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;

        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _accountService.GetAllAsync();
            return Ok(accounts);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedCategories([FromQuery] AccountFilters filters)
        {
            var paginatedAccounts = await _accountService.GetPaginatedAsync(filters);
            return Ok(paginatedAccounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            var account = await _accountService.GetByIdAsync(id);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] AccountDto dto)
        {
            var account = await _accountService.CreateAsync(dto);
            return Ok(account);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateAccount(Guid id)
        {
            await _accountService.DeactivateAsync(id);
            return NoContent();
        }
    }

}
