using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Leaderboard;

namespace Web.Infrastructure.EntityConfigurations
{
    public class HallOfFameConfiguration : IEntityTypeConfiguration<HallOfFame>
    {
        public void Configure(EntityTypeBuilder<HallOfFame> builder)
        {
            builder.ToTable("hall_of_fames");
            
            builder.HasKey(h => h.Id);
            
            // TPH discriminator configuration
            builder.HasDiscriminator<string>("hall_of_fame_type")
                .HasValue<HallOfFame>("base")
                .HasValue<MonthHallOfFame>("month");
            
            // Base HallOfFame properties
            builder.Property(h => h.Id)
                .HasColumnName("id")
                .IsRequired();
            
            builder.Property(h => h.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
            
            // Owned entity: MostBirdies
            builder.OwnsOne(h => h.MostBirdies, mb =>
            {
                mb.Property(x => x.Count)
                    .HasColumnName("most_birdies_count")
                    .IsRequired();
                mb.Property(x => x.PerRound)
                    .HasColumnName("most_birdies_per_round")
                    .IsRequired();
                mb.Property(x => x.Username)
                    .HasColumnName("most_birdies_username")
                    .IsRequired();
                mb.Property(x => x.TimeOfEntry)
                    .HasColumnName("most_birdies_time_of_entry")
                    .IsRequired();
                mb.Property(x => x.NewThisMonth)
                    .HasColumnName("most_birdies_new_this_month")
                    .IsRequired();
                mb.Ignore(x => x.DaysInHallOfFame);
            });
            
            // Owned entity: MostBogies
            builder.OwnsOne(h => h.MostBogies, mb =>
            {
                mb.Property(x => x.Count)
                    .HasColumnName("most_bogies_count")
                    .IsRequired();
                mb.Property(x => x.PerRound)
                    .HasColumnName("most_bogies_per_round")
                    .IsRequired();
                mb.Property(x => x.Username)
                    .HasColumnName("most_bogies_username")
                    .IsRequired();
                mb.Property(x => x.TimeOfEntry)
                    .HasColumnName("most_bogies_time_of_entry")
                    .IsRequired();
                mb.Property(x => x.NewThisMonth)
                    .HasColumnName("most_bogies_new_this_month")
                    .IsRequired();
                mb.Ignore(x => x.DaysInHallOfFame);
            });
            
            // Owned entity: MostRounds
            builder.OwnsOne(h => h.MostRounds, mr =>
            {
                mr.Property(x => x.Count)
                    .HasColumnName("most_rounds_count")
                    .IsRequired();
                mr.Property(x => x.Username)
                    .HasColumnName("most_rounds_username")
                    .IsRequired();
                mr.Property(x => x.TimeOfEntry)
                    .HasColumnName("most_rounds_time_of_entry")
                    .IsRequired();
                mr.Property(x => x.NewThisMonth)
                    .HasColumnName("most_rounds_new_this_month")
                    .IsRequired();
                mr.Ignore(x => x.DaysInHallOfFame);
            });
            
            // Owned entity: BestRoundAverage
            builder.OwnsOne(h => h.BestRoundAverage, bra =>
            {
                bra.Property(x => x.RoundAverage)
                    .HasColumnName("best_round_average_round_average")
                    .IsRequired();
                bra.Property(x => x.Username)
                    .HasColumnName("best_round_average_username")
                    .IsRequired();
                bra.Property(x => x.TimeOfEntry)
                    .HasColumnName("best_round_average_time_of_entry")
                    .IsRequired();
                bra.Property(x => x.NewThisMonth)
                    .HasColumnName("best_round_average_new_this_month")
                    .IsRequired();
                bra.Ignore(x => x.DaysInHallOfFame);
            });
            
            // MonthHallOfFame-specific properties
            builder.Property<int>("Month")
                .HasColumnName("month");
            
            builder.Property<int>("Year")
                .HasColumnName("year");
            
            builder.Property<DateTime>("CreatedAt")
                .HasColumnName("created_at");
        }
    }
}
