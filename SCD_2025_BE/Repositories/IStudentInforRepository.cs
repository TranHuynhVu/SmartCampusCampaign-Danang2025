using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public interface IStudentInforRepository : IGenericRepository<StudentInfor>
    {
        Task<StudentInfor?> GetByUserIdAsync(string userId);
        Task<IEnumerable<StudentInfor>> GetActiveStudentsAsync();
    }
}
