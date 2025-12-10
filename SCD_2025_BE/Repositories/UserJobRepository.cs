using Microsoft.EntityFrameworkCore;
using SCD_2025_BE.Data;
using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public class UserJobRepository : GenericRepository<UserJob>, IUserJobRepository
    {
        public UserJobRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserJob>> GetApplicationsByUserIdAsync(string userId)
        {
            return await _context.UserJobs
                .Include(uj => uj.Job)
                    .ThenInclude(j => j.CompanyInfor)
                .Where(uj => uj.UserId == userId && uj.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserJob>> GetApplicationsByJobIdAsync(int jobId)
        {
            return await _context.UserJobs
                .Include(uj => uj.Job)
                    .ThenInclude(j => j.CompanyInfor)
                .Where(uj => uj.JobId == jobId && uj.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<UserJob?> GetApplicationAsync(string userId, int jobId)
        {
            return await _context.UserJobs
                .FirstOrDefaultAsync(uj => uj.UserId == userId && uj.JobId == jobId && uj.DeletedAt == null);
        }

        public async Task<UserJob?> GetApplicationWithDetailsAsync(int id)
        {
            return await _context.UserJobs
                .Include(uj => uj.Job)
                    .ThenInclude(j => j.CompanyInfor)
                .FirstOrDefaultAsync(uj => uj.Id == id && uj.DeletedAt == null);
        }
    }
}
