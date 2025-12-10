namespace SCD_2025_BE.Repositories
{
    public interface IUnitOfWork : IDisposable
    {   
        Task<int> SaveChangesAsync();
    }
}
