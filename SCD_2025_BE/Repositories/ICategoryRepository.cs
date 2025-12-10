using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    }
}
