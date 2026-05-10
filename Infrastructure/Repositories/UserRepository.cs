using Microsoft.EntityFrameworkCore;
using MyFinances.Api.DTOs;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;
using MyFinances.Infrastructure.Data;
using MyFinances.Infrastructure.Repositories.Interfaces;

namespace MyFinances.Infrastructure.Repositories
{
    public class UserRepository(FinanceDbContext context) : Repository<User>(context), IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByGoogleSubjectIdAsync(string googleSubjectId)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.GoogleSubjectId == googleSubjectId);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PagedResultBase<User>> GetAllFilteredAsync(AdminUserFilterDto filters, Guid excludeUserId)
        {
            var query = _dbSet.Where(u => u.Id != excludeUserId);

            if (!string.IsNullOrWhiteSpace(filters.FullName))
                query = query.Where(u => u.FullName.ToLower().Contains(filters.FullName.ToLower()));

            if (!string.IsNullOrWhiteSpace(filters.Nickname))
                query = query.Where(u => u.Nickname != null && u.Nickname.ToLower().Contains(filters.Nickname.ToLower()));

            if (!string.IsNullOrWhiteSpace(filters.DocumentNumber))
                query = query.Where(u => u.DocumentNumber != null && u.DocumentNumber.ToLower().Contains(filters.DocumentNumber.ToLower()));

            if (filters.Role.HasValue)
                query = query.Where(u => u.Role == filters.Role.Value);

            if (filters.CreatedAtFrom.HasValue)
                query = query.Where(u => u.CreatedAt >= filters.CreatedAtFrom.Value);

            if (filters.CreatedAtTo.HasValue)
                query = query.Where(u => u.CreatedAt <= filters.CreatedAtTo.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return new PagedResultBase<User>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
