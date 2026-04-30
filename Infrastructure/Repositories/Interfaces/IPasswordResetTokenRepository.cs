using MyFinances.Domain.Entities;

namespace MyFinances.Infrastructure.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
    {
        Task<IEnumerable<PasswordResetToken>> GetValidTokensByUserIdAsync(Guid userId);
    }
}
