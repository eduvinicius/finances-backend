using Microsoft.EntityFrameworkCore.Storage;
using MyFinances.Infrasctructure.Data;
using MyFinances.Infrasctructure.Repositories.Interfaces;

namespace MyFinances.Infrasctructure.Repositories
{
    public class UnitOfWork(
        FinanceDbContext context,
        IUserRepository userRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository) : IUnitOfWork
    {

        private readonly FinanceDbContext _context = context;

        private IDbContextTransaction? _transaction;
        public IUserRepository Users { get; } = userRepository;
        public IAccountRepository Accounts { get; } = accountRepository;
        public ICategoryRepository Categories { get; } = categoryRepository;
        public ITransactionRepository Transactions { get; } = transactionRepository;

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction!.CommitAsync();
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
