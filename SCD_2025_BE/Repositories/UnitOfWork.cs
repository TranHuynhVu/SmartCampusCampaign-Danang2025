using SCD_2025_BE.Repositories;
using SCD_2025_BE.Data;

namespace SCD_2025_BE.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private ICategoryRepository _categories;
        private ICompanyInforRepository _companyInfors;
        private IJobRepository _jobs;
        private IStudentInforRepository _studentInfors;
        private IUserJobRepository _userJobs;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public ICompanyInforRepository CompanyInfors => _companyInfors ??= new CompanyInforRepository(_context);
        public IJobRepository Jobs => _jobs ??= new JobRepository(_context);
        public IStudentInforRepository StudentInfors => _studentInfors ??= new StudentInforRepository(_context);
        public IUserJobRepository UserJobs => _userJobs ??= new UserJobRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
