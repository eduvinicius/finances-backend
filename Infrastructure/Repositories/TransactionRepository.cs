using Microsoft.EntityFrameworkCore;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.App.Utils;
using MyFinances.Domain.Entities;
using MyFinances.Infrastructure.Data;
using MyFinances.Infrastructure.Repositories.Interfaces;

namespace MyFinances.Infrastructure.Repositories
{
    public class TransactionRepository(FinanceDbContext context) : Repository<Transaction>(context), ITransactionRepository
    {
        public async Task<PagedResultBase<Transaction>> GetAllTransactionsAsync(Guid userId, TransactionFilters filters)
        {

            var query = _context.Transactions
                .Where(t => t.UserId == userId);

            if (filters.AccountIds.Count != 0)
                query = query.Where(t => filters.AccountIds.Contains(t.AccountId.ToString()));

            if (filters.CategoryIds.Count != 0)
                query = query.Where(t => filters.CategoryIds.Contains(t.CategoryId.ToString()));

            if (filters.Type.Count != 0)
                query = query.Where(c => filters.Type.Contains((int)c.Type));

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filters.ToDate.Value);

            var totalCount = await query.CountAsync();

            var items =  await query
                .AsNoTracking()
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Include(t => t.Account)
                .Include(t => t.Category)
                .ToListAsync();

            return new PagedResultBase<Transaction>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
