using Microsoft.EntityFrameworkCore;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.App.Utils;
using MyFinances.Domain.Entities;
using MyFinances.Infrasctructure.Data;
using MyFinances.Infrasctructure.Repositories.Interfaces;

namespace MyFinances.Infrasctructure.Repositories
{
    public class AccountRepository(FinanceDbContext context) : Repository<Account>(context), IAccountRepository
    {
        public async Task<PagedResultBase<Account>> GetPaginatedByUserIdAsync(Guid userId, AccountFilters filters)
        {

            var typeEnums = ConvertStringToArrayEnum.Convert(filters.Type);
            var query = _context.Accounts
                .Where(a => a.UserId == userId);

            if (!string.IsNullOrEmpty(filters.Name))
                query = query.Where(a => a.Name.Contains(filters.Name));

            if (typeEnums.Count != 0)
                query = query.Where(c => typeEnums.Contains((int)c.Type));

            if (filters.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filters.ToDate.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .AsNoTracking()
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return new PagedResultBase<Account>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
