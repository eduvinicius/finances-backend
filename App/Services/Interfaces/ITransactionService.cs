using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResultBase<TransactionResponseDto>> GetAllByUserId(TransactionFilters filters);
        Task<TransactionResponseDto> GetByIdAsync(Guid transactionId);
        Task<Transaction> CreateAsync(TransactionDto dto);
    }
}
