namespace MyFinances.Infrasctructure.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }

        Task<int> SaveChangesAsync();
    }
}
