using Microsoft.EntityFrameworkCore;
using SCD_2025_BE.Data;
using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public class CompanyInforRepository : GenericRepository<CompanyInfor>, ICompanyInforRepository
    {
        public CompanyInforRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<CompanyInfor?> GetByUserIdAsync(string userId)
        {
            return await _context.CompanyInfors
                .FirstOrDefaultAsync(c => c.UserId == userId && c.DeletedAt == null);
        }

        public async Task<IEnumerable<CompanyInfor>> GetActiveCompaniesAsync()
        {
            return await _context.CompanyInfors
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
        }
    }
}
