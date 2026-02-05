using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllByUserId(TransactionFilters filters);
        Task<Transaction> GetByIdAsync(Guid transactionId);
        Task<Transaction> CreateAsync(TransactionDto dto);
    }
}
