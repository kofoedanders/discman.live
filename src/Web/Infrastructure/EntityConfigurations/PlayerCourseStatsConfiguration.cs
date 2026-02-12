using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Rounds;

namespace Web.Infrastructure.EntityConfigurations
{
    public class PlayerCourseStatsConfiguration : IEntityTypeConfiguration<PlayerCourseStats>
    {
        public void Configure(EntityTypeBuilder<PlayerCourseStats> builder)
        {
            builder.ToTable("player_course_stats");
            
            // Keyless entity (read model/view, not persisted by EF Core)
            builder.HasNoKey();
            
            // PostgreSQL array columns for double precision
            builder.Property(x => x.HoleAverages)
                .HasColumnType("double precision[]")
                .HasColumnName("hole_averages");
            
            builder.Property(x => x.AveragePrediction)
                .HasColumnType("double precision[]")
                .HasColumnName("average_prediction");
            
            // JSONB column for nested HoleStats list (not normalized)
            builder.Property(x => x.HoleStats)
                .HasColumnType("jsonb")
                .HasColumnName("hole_stats");
            
            // Standard columns with snake_case naming
            builder.Property(x => x.CourseName)
                .HasColumnName("course_name");
            
            builder.Property(x => x.LayoutName)
                .HasColumnName("layout_name");
            
            builder.Property(x => x.PlayerName)
                .HasColumnName("player_name");
            
            builder.Property(x => x.CourseAverage)
                .HasColumnName("course_average");
            
            builder.Property(x => x.PlayerCourseRecord)
                .HasColumnName("player_course_record");
            
            builder.Property(x => x.ThisRoundVsAverage)
                .HasColumnName("this_round_vs_average");
            
            builder.Property(x => x.RoundsPlayed)
                .HasColumnName("rounds_played");
        }
    }
}
