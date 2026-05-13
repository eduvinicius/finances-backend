using Microsoft.EntityFrameworkCore;
using MyFinances.App.Abstractions;
using MyFinances.Infrastructure.Data;

namespace MyFinances.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly FinanceDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(FinanceDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        protected virtual IQueryable<T> WithIncludes()
        {
            return _dbSet;
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task<IEnumerable<T>> GetAllByUserIdAsync(Guid userId)
        {
            return await WithIncludes()
                .Where(e => EF.Property<Guid>(e, "UserId") == userId)
                .ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await WithIncludes()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

    }
}
