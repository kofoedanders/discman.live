using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Feeds.Domain;

namespace Web.Infrastructure.EntityConfigurations
{
    public class GlobalFeedItemConfiguration : IEntityTypeConfiguration<GlobalFeedItem>
    {
        public void Configure(EntityTypeBuilder<GlobalFeedItem> builder)
        {
            builder.ToTable("global_feed_items");
            builder.HasKey(x => x.Id);
            
            // Index on RegisteredAt for feed query performance
            builder.HasIndex(x => x.RegisteredAt);
            
            // PostgreSQL array columns
            builder.Property(x => x.Likes)
                .HasColumnType("text[]")
                .HasColumnName("likes");
            
            builder.Property(x => x.Subjects)
                .HasColumnType("text[]")
                .HasColumnName("subjects");
            
            builder.Property(x => x.RoundScores)
                .HasColumnType("integer[]")
                .HasColumnName("round_scores");
            
            // Standard columns with snake_case naming
            builder.Property(x => x.ItemType)
                .HasColumnName("item_type");
            
            builder.Property(x => x.AchievementName)
                .HasColumnName("achievement_name");
            
            builder.Property(x => x.RegisteredAt)
                .HasColumnName("registered_at");
            
            builder.Property(x => x.CourseName)
                .HasColumnName("course_name");
            
            builder.Property(x => x.HoleScore)
                .HasColumnName("hole_score");
            
            builder.Property(x => x.HoleNumber)
                .HasColumnName("hole_number");
            
            builder.Property(x => x.Action)
                .HasColumnName("action");
            
            builder.Property(x => x.RoundId)
                .HasColumnName("round_id");
            
            builder.Property(x => x.TournamentId)
                .HasColumnName("tournament_id");
            
            builder.Property(x => x.TournamentName)
                .HasColumnName("tournament_name");
            
            builder.Property(x => x.FriendName)
                .HasColumnName("friend_name");
        }
    }
}
