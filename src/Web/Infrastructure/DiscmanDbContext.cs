using Microsoft.EntityFrameworkCore;
using Web.Courses;
using Web.Feeds.Domain;
using Web.Leaderboard;
using Web.Rounds;
using Web.Tournaments.Domain;
using Web.Users.Domain;

namespace Web.Infrastructure
{
    /// <summary>EF Core DbContext for Discman app. Replaces Marten document store.</summary>
    public class DiscmanDbContext : DbContext
    {
        public DiscmanDbContext(DbContextOptions<DiscmanDbContext> options)
            : base(options)
        {
        }

        // 8 Marten document types
        public DbSet<Web.Users.User> Users { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<GlobalFeedItem> GlobalFeedItems { get; set; }
        public DbSet<UserFeedItem> UserFeedItems { get; set; }
        public DbSet<HallOfFame> HallOfFames { get; set; }
        public DbSet<MonthHallOfFame> MonthHallOfFames { get; set; }
        public DbSet<ResetPasswordRequest> ResetPasswordRequests { get; set; }
        public DbSet<PlayerCourseStats> PlayerCourseStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Entity configurations will be added in sub-tasks 2c-2e
        }
    }
}
