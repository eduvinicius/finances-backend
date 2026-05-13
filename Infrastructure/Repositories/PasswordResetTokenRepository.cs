using Microsoft.EntityFrameworkCore;
using MyFinances.Domain.Entities;
using MyFinances.Infrastructure.Data;
using MyFinances.App.Abstractions;

namespace MyFinances.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository(FinanceDbContext context)
        : Repository<PasswordResetToken>(context), IPasswordResetTokenRepository
    {
        public async Task<IEnumerable<PasswordResetToken>> GetValidTokensByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(t => t.UserId == userId && !t.Used && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
