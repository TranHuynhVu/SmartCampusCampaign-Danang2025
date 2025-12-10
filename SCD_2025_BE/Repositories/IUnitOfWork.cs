namespace SCD_2025_BE.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        ICompanyInforRepository CompanyInfors { get; }
        IJobRepository Jobs { get; }
        IStudentInforRepository StudentInfors { get; }
        IUserJobRepository UserJobs { get; }
        Task<int> SaveChangesAsync();
    }
}
