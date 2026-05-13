using Microsoft.EntityFrameworkCore.Storage;
using MyFinances.Infrastructure.Data;
using MyFinances.App.Abstractions;

namespace MyFinances.Infrastructure.Repositories
{
    public class UnitOfWork(
        FinanceDbContext context,
        IUserRepository userRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository) : IUnitOfWork
    {

        private readonly FinanceDbContext _context = context;

        private IDbContextTransaction? _transaction;
        public IUserRepository Users { get; } = userRepository;
        public IAccountRepository Accounts { get; } = accountRepository;
        public ICategoryRepository Categories { get; } = categoryRepository;
        public ITransactionRepository Transactions { get; } = transactionRepository;
        public IPasswordResetTokenRepository Tokens { get; } = passwordResetTokenRepository;

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to commit. Call BeginTransactionAsync() first.");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                await _transaction?.RollbackAsync()!;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
             await _context.SaveChangesAsync();
        }

        public async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
