using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public interface ICompanyInforRepository : IGenericRepository<CompanyInfor>
    {
        Task<CompanyInfor?> GetByUserIdAsync(string userId);
        Task<IEnumerable<CompanyInfor>> GetActiveCompaniesAsync();
    }
}
