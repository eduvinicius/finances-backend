using Microsoft.EntityFrameworkCore;
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
    }
}
