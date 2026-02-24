using MyFinances.App.Shared;
using MyFinances.Domain.Entities;

namespace MyFinances.Infrasctructure.Repositories.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<PagedResultBase<Transaction>> GetAllTransactionsAsync(Guid userId, App.Filters.TransactionFilters filters);
    }
}
