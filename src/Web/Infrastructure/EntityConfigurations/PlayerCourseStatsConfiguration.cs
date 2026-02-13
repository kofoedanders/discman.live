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
            builder.HasNoKey();

            builder.Property(x => x.PlayerName)
                .HasColumnName("player_name");

            builder.Property(x => x.CourseName)
                .HasColumnName("course_name");

            builder.Property(x => x.LayoutName)
                .HasColumnName("layout_name");

            builder.Property(x => x.CourseAverage)
                .HasColumnName("course_average");

            builder.Property(x => x.PlayerCourseRecord)
                .HasColumnName("player_course_record");

            builder.Property(x => x.ThisRoundVsAverage)
                .HasColumnName("this_round_vs_average");

            builder.Property(x => x.HoleAverages)
                .HasColumnName("hole_averages")
                .HasColumnType("double precision[]");

            builder.Property(x => x.AveragePrediction)
                .HasColumnName("average_prediction")
                .HasColumnType("double precision[]");

            builder.Property(x => x.RoundsPlayed)
                .HasColumnName("rounds_played");

            builder.Property(x => x.HoleStats)
                .HasColumnName("hole_stats")
                .HasColumnType("jsonb");
        }
    }
}
