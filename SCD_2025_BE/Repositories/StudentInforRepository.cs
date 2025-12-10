using Microsoft.EntityFrameworkCore;
using SCD_2025_BE.Data;
using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public class StudentInforRepository : GenericRepository<StudentInfor>, IStudentInforRepository
    {
        public StudentInforRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<StudentInfor?> GetByUserIdAsync(string userId)
        {
            return await _context.StudentInfors
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeletedAt == null);
        }

        public async Task<IEnumerable<StudentInfor>> GetActiveStudentsAsync()
        {
            return await _context.StudentInfors
                .Where(s => s.DeletedAt == null)
                .ToListAsync();
        }
    }
}
