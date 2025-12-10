using Microsoft.EntityFrameworkCore;
using SCD_2025_BE.Data;
using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public class JobRepository : GenericRepository<Job>, IJobRepository
    {
        public JobRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Job>> GetActiveJobsAsync(string? status = null, int? categoryId = null, string? location = null)
        {
            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.CompanyInfor)
                .Where(j => j.DeletedAt == null);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(j => j.Status == status);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(j => j.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(j => j.Location.Contains(location));
            }

            return await query.ToListAsync();
        }

        public async Task<Job?> GetJobWithDetailsAsync(int id)
        {
            return await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.CompanyInfor)
                .FirstOrDefaultAsync(j => j.Id == id && j.DeletedAt == null);
        }

        public async Task<IEnumerable<Job>> GetJobsByCompanyIdAsync(int companyId)
        {
            return await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.CompanyInfor)
                .Where(j => j.CompanyInforId == companyId && j.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetJobsByUserIdAsync(string userId)
        {
            return await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.CompanyInfor)
                .Where(j => j.CompanyInfor.UserId == userId && j.DeletedAt == null)
                .ToListAsync();
        }
    }
}
