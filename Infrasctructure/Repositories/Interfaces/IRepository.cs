namespace MyFinances.Infrasctructure.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task<T?> GetByIdAsync(int id);
        void Update(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }
}
