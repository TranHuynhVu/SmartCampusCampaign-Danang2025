using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CompanyInfor> CompanyInfors { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<StudentInfor> StudentInfors { get; set; }
        public DbSet<UserJob> UserJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<CompanyInfor>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentInfor>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Category)
                .WithMany()
                .HasForeignKey(j => j.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.CompanyInfor)
                .WithMany()
                .HasForeignKey(j => j.CompanyInforId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserJob>()
                .HasOne(uj => uj.User)
                .WithMany()
                .HasForeignKey(uj => uj.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserJob>()
                .HasOne(uj => uj.Job)
                .WithMany()
                .HasForeignKey(uj => uj.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
