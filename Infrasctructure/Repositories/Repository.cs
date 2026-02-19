using Microsoft.EntityFrameworkCore;
using MyFinances.Infrasctructure.Data;
using MyFinances.Infrasctructure.Repositories.Interfaces;

namespace MyFinances.Infrasctructure.Repositories
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
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
