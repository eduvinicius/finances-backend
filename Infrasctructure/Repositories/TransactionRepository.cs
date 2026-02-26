using Microsoft.EntityFrameworkCore;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.App.Utils;
using MyFinances.Domain.Entities;
using MyFinances.Infrasctructure.Data;
using MyFinances.Infrasctructure.Repositories.Interfaces;

namespace MyFinances.Infrasctructure.Repositories
{
    public class TransactionRepository(FinanceDbContext context) : Repository<Transaction>(context), ITransactionRepository
    {
        public async Task<PagedResultBase<Transaction>> GetAllTransactionsAsync(Guid userId, TransactionFilters filters)
        {

            var typeEnums = ConvertStringToArrayEnum.Convert(filters.Type);

            var query = _context.Transactions
                .Where(t => t.UserId == userId);

            if (filters.AccountId.Count != 0)
                query = query.Where(t => filters.AccountId.Contains(t.AccountId.ToString()));

            if (filters.CategoryId.Count != 0)
                query = query.Where(t => filters.CategoryId.Contains(t.CategoryId.ToString()));

            if (typeEnums.Count != 0)
                query = query.Where(c => typeEnums.Contains((int)c.Type));

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.Date >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.Date <= filters.ToDate.Value);

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
