using MyFinances.Domain.Entities;

namespace MyFinances.App.Abstractions
{
    public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
    {
        Task<IEnumerable<PasswordResetToken>> GetValidTokensByUserIdAsync(Guid userId);
    }
}
