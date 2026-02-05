using MyFinances.Domain.Entities;

namespace MyFinances.Infrasctructure.Repositories.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync(Guid userId, App.Filters.TransactionFilters filters);
    }
}
