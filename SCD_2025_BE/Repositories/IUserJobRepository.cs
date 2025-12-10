using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public interface IUserJobRepository : IGenericRepository<UserJob>
    {
        Task<IEnumerable<UserJob>> GetApplicationsByUserIdAsync(string userId);
        Task<IEnumerable<UserJob>> GetApplicationsByJobIdAsync(int jobId);
        Task<UserJob?> GetApplicationAsync(string userId, int jobId);
        Task<UserJob?> GetApplicationWithDetailsAsync(int id);
    }
}
