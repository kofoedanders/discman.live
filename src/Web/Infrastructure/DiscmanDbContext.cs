using Microsoft.EntityFrameworkCore;
using Web.Courses;
using Web.Feeds.Domain;
using Web.Infrastructure.EntityConfigurations;
using Web.Leaderboard;
using Web.Rounds;
using Web.Tournaments.Domain;
using Web.Users.Domain;

namespace Web.Infrastructure
{
    public class DiscmanDbContext : DbContext
    {
        public DiscmanDbContext(DbContextOptions<DiscmanDbContext> options)
            : base(options)
        {
        }

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
            // Apply all IEntityTypeConfiguration classes from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiscmanDbContext).Assembly);

            // Exclude Achievement types from model discovery (they are domain value objects, not entities)
            modelBuilder.Ignore<Web.Users.Achievement>();
            modelBuilder.Ignore<Web.Users.RoundAchievement>();
            modelBuilder.Ignore<Web.Users.UserAchievement>();
            modelBuilder.Ignore<Web.Users.BogeyFreeRound>();
            modelBuilder.Ignore<Web.Users.AllPar>();
            modelBuilder.Ignore<Web.Users.UnderPar>();
            modelBuilder.Ignore<Web.Users.FiveUnderPar>();
            modelBuilder.Ignore<Web.Users.TenUnderPar>();
            modelBuilder.Ignore<Web.Users.StarFrame>();
            modelBuilder.Ignore<Web.Users.FiveBirdieRound>();
            modelBuilder.Ignore<Web.Users.ACE>();
            modelBuilder.Ignore<Web.Users.Turkey>();
            modelBuilder.Ignore<Web.Users.Eagle>();
            modelBuilder.Ignore<Web.Users.TenRoundsInAMonth>();
            modelBuilder.Ignore<Web.Users.TwentyRoundsInAMonth>();
            modelBuilder.Ignore<Web.Users.PlayEveryDayInAWeek>();
            modelBuilder.Ignore<Web.Users.OneHundredRounds>();
            modelBuilder.Ignore<Web.Users.FiveRoundsInADay>();
        }
    }
}
