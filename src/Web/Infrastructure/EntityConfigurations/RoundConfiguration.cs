using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Rounds;

namespace Web.Infrastructure.EntityConfigurations
{
    public class RoundConfiguration : IEntityTypeConfiguration<Round>
    {
        public void Configure(EntityTypeBuilder<Round> builder)
        {
            builder.ToTable("rounds");
            builder.HasKey(r => r.Id);
            builder.HasQueryFilter(r => !r.Deleted);
            
            builder.Property(r => r.Id)
                .HasColumnName("id")
                .IsRequired();
            
            builder.Property(r => r.Spectators)
                .HasColumnName("spectators")
                .HasColumnType("text[]")
                .IsRequired();
            
            builder.Property(r => r.ScoreMode)
                .HasColumnName("score_mode")
                .IsRequired();
            
            builder.Property(r => r.RoundName)
                .HasColumnName("round_name");
            
            builder.Property(r => r.CourseName)
                .HasColumnName("course_name");
            
            builder.Property(r => r.CourseLayout)
                .HasColumnName("course_layout");
            
            builder.Property(r => r.CourseId)
                .HasColumnName("course_id")
                .IsRequired();
            
            builder.Property(r => r.StartTime)
                .HasColumnName("start_time")
                .IsRequired();
            
            builder.Property(r => r.IsCompleted)
                .HasColumnName("is_completed")
                .IsRequired();
            
            builder.Property(r => r.CompletedAt)
                .HasColumnName("completed_at")
                .IsRequired();
            
            builder.Property(r => r.CreatedBy)
                .HasColumnName("created_by")
                .IsRequired();
            
            builder.Property(r => r.Deleted)
                .HasColumnName("deleted")
                .IsRequired();
            
            builder.Ignore(r => r.DurationMinutes);
            builder.Ignore(r => r.Achievements);
            
            builder.OwnsMany(r => r.Signatures, signature =>
            {
                signature.ToTable("player_signatures");
                signature.WithOwner().HasForeignKey("RoundId");
                signature.Property<int>("Id");
                signature.HasKey("Id");
                
                signature.Property(s => s.Username)
                    .HasColumnName("username")
                    .IsRequired();
                signature.Property(s => s.Base64Signature)
                    .HasColumnName("base64_signature")
                    .IsRequired();
                signature.Property(s => s.SignedAt)
                    .HasColumnName("signed_at")
                    .IsRequired();
            });
            
            builder.OwnsMany(r => r.RatingChanges, rating =>
            {
                rating.ToTable("rating_changes");
                rating.WithOwner().HasForeignKey("RoundId");
                rating.Property<int>("Id");
                rating.HasKey("Id");
                
                rating.Property(rc => rc.Username)
                    .HasColumnName("username")
                    .IsRequired();
                rating.Property(rc => rc.Change)
                    .HasColumnName("change")
                    .IsRequired();
            });
            
            builder.OwnsMany(r => r.PlayerScores, playerScore =>
            {
                playerScore.ToTable("player_scores");
                playerScore.WithOwner().HasForeignKey("RoundId");
                playerScore.Property<int>("Id");
                playerScore.HasKey("Id");
                
                playerScore.Property(ps => ps.PlayerName)
                    .HasColumnName("player_name")
                    .IsRequired();
                playerScore.Property(ps => ps.PlayerEmoji)
                    .HasColumnName("player_emoji");
                playerScore.Property(ps => ps.PlayerRoundStatusEmoji)
                    .HasColumnName("player_round_status_emoji");
                playerScore.Property(ps => ps.CourseAverageAtTheTime)
                    .HasColumnName("course_average_at_the_time")
                    .IsRequired();
                playerScore.Property(ps => ps.NumberOfHcpStrokes)
                    .HasColumnName("number_of_hcp_strokes")
                    .IsRequired();
                
                playerScore.OwnsMany(ps => ps.Scores, holeScore =>
                {
                    holeScore.ToTable("hole_scores");
                    holeScore.WithOwner().HasForeignKey("PlayerScoreId");
                    holeScore.Property<int>("Id");
                    holeScore.HasKey("Id");
                    
                    holeScore.Property(hs => hs.Strokes)
                        .HasColumnName("strokes")
                        .IsRequired();
                    holeScore.Property(hs => hs.RelativeToPar)
                        .HasColumnName("relative_to_par")
                        .IsRequired();
                    holeScore.Property(hs => hs.RegisteredAt)
                        .HasColumnName("registered_at")
                        .IsRequired();
                    
                    holeScore.OwnsOne(hs => hs.Hole, hole =>
                    {
                        hole.Property(x => x.Number)
                            .HasColumnName("hole_number")
                            .IsRequired();
                        hole.Property(x => x.Par)
                            .HasColumnName("hole_par")
                            .IsRequired();
                        hole.Property(x => x.Distance)
                            .HasColumnName("hole_distance")
                            .IsRequired();
                        hole.Property(x => x.Average)
                            .HasColumnName("hole_average")
                            .IsRequired();
                        hole.Property(x => x.Rating)
                            .HasColumnName("hole_rating")
                            .IsRequired();
                    });
                    
                    holeScore.OwnsMany(hs => hs.StrokeSpecs, strokeSpec =>
                    {
                        strokeSpec.ToTable("stroke_specs");
                        strokeSpec.WithOwner().HasForeignKey("HoleScoreId");
                        strokeSpec.Property<int>("Id");
                        strokeSpec.HasKey("Id");
                        
                        strokeSpec.Property(ss => ss.Outcome)
                            .HasColumnName("outcome")
                            .IsRequired();
                        strokeSpec.Property(ss => ss.PutDistance)
                            .HasColumnName("put_distance");
                    });
                });
            });
        }
    }
}
