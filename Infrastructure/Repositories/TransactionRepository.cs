using Microsoft.EntityFrameworkCore;
using MyFinances.Api.DTOs;
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

        public async Task<IReadOnlyList<Transaction>> GetForExportAsync(Guid userId, TransactionExportDto filters)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId);

            if (!filters.ExportAll)
            {
                if (filters.StartDate.HasValue)
                    query = query.Where(t => t.CreatedAt >= filters.StartDate.Value);

                if (filters.EndDate.HasValue)
                    query = query.Where(t => t.CreatedAt <= filters.EndDate.Value);

                if (filters.CategoryId.HasValue)
                    query = query.Where(t => t.CategoryId == filters.CategoryId.Value);

                if (filters.AccountId.HasValue)
                    query = query.Where(t => t.AccountId == filters.AccountId.Value);

                if (filters.Type.HasValue)
                    query = query.Where(t => t.Type == filters.Type.Value);
            }

            return await query
                .AsNoTracking()
                .Include(t => t.Account)
                .Include(t => t.Category)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
