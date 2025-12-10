using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public interface IJobRepository : IGenericRepository<Job>
    {
        Task<IEnumerable<Job>> GetActiveJobsAsync(string? status = null, int? categoryId = null, string? location = null);
        Task<Job?> GetJobWithDetailsAsync(int id);
        Task<IEnumerable<Job>> GetJobsByCompanyIdAsync(int companyId);
        Task<IEnumerable<Job>> GetJobsByUserIdAsync(string userId);
    }
}
