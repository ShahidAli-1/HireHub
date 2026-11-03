using HireHub.Models;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
        public DbSet<RecruiterProfile> RecruiterProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DailyUsage> DailyUsages { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // One application per job per user
            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.JobId, a.JobSeekerId })
                .IsUnique();

            // One daily usage row per user per day
            modelBuilder.Entity<DailyUsage>()
                .HasIndex(d => new { d.UserId, d.Date })
                .IsUnique();

            // User -> JobSeekerProfile (1-to-1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.JobSeekerProfile)
                .WithOne(p => p.User)
                .HasForeignKey<JobSeekerProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> RecruiterProfile (1-to-1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.RecruiterProfile)
                .WithOne(p => p.User)
                .HasForeignKey<RecruiterProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Jobs (1-to-many)
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Recruiter)
                .WithMany(u => u.Jobs)
                .HasForeignKey(j => j.RecruiterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job -> Applications (1-to-many)
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Applications (preserve history, NO cascade)
            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobSeeker)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.JobSeekerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Payment reviewer
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reviewer)
                .WithMany()
                .HasForeignKey(p => p.ReviewedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Prevent multiple cascade paths for SavedJobs
            modelBuilder.Entity<SavedJob>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}