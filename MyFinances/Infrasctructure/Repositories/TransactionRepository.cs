using Microsoft.EntityFrameworkCore;
using MyFinances.App.Filters;
using MyFinances.Domain.Entities;
using MyFinances.Infrasctructure.Data;
using MyFinances.Infrasctructure.Repositories.Interfaces;

namespace MyFinances.Infrasctructure.Repositories
{
    public class TransactionRepository(FinanceDbContext context) : Repository<Transaction>(context), ITransactionRepository
    {
        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(Guid userId, TransactionFilters filters)
        {
            var query = WithIncludes()
                .Where(t => t.UserId == userId);

            if (filters.AccountId.HasValue)
                 query = query.Where(t => t.AccountId == filters.AccountId.Value);

            if (filters.CategoryId.HasValue)
                 query = query.Where(t => t.CategoryId == filters.CategoryId.Value);

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.Date >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.Date <= filters.ToDate.Value);

            return await query
                .Skip(filters.Page)
                .Take(filters.PageSize)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedAt)
                .Include(t => t.Account)
                .Include(t => t.Category)
                .ToListAsync();
        }
    }
}
