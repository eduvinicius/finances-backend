using Microsoft.EntityFrameworkCore;
using MyFinances.App.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.App.Utils;
using MyFinances.Domain.Entities;
using MyFinances.Infrastructure.Data;
using MyFinances.App.Abstractions;

namespace MyFinances.Infrastructure.Repositories
{
    public class TransactionRepository(FinanceDbContext context) : Repository<Transaction>(context), ITransactionRepository
    {
        public async Task<PagedResultBase<Transaction>> GetAllTransactionsAsync(Guid userId, TransactionFilters filters)
        {

            var query = _context.Transactions
                .Where(t => t.UserId == userId);

            if (filters.AccountIds.Count != 0)
            {
                var accountGuids = filters.AccountIds
                    .Select(id => Guid.TryParse(id, out var g) ? g : (Guid?)null)
                    .Where(g => g.HasValue)
                    .Select(g => g!.Value)
                    .ToList();
                query = query.Where(t => accountGuids.Contains(t.AccountId));
            }

            if (filters.CategoryIds.Count != 0)
            {
                var categoryGuids = filters.CategoryIds
                    .Select(id => Guid.TryParse(id, out var g) ? g : (Guid?)null)
                    .Where(g => g.HasValue)
                    .Select(g => g!.Value)
                    .ToList();
                query = query.Where(t => categoryGuids.Contains(t.CategoryId));
            }

            if (filters.Type.Count != 0)
                query = query.Where(c => filters.Type.Contains(c.Type));

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filters.ToDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .Include(t => t.Account)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return new PagedResultBase<Transaction>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<Transaction?> GetByIdAsync(Guid transactionId, Guid userId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Account)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);
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
