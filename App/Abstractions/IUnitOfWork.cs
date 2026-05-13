namespace MyFinances.App.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IAccountRepository Accounts { get; }
        ICategoryRepository Categories { get; }
        ITransactionRepository Transactions { get; }
        IPasswordResetTokenRepository Tokens { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task SaveChangesAsync();
        Task DisposeTransactionAsync();
    }
}
