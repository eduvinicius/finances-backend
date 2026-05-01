using MyFinances.Api.DTOs;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.Infrastructure.Repositories.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<PagedResultBase<Transaction>> GetAllTransactionsAsync(Guid userId, App.Filters.TransactionFilters filters);
        Task<IReadOnlyList<Transaction>> GetForExportAsync(Guid userId, TransactionExportDto filters);
    }
}
