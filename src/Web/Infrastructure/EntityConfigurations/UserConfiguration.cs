using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Web.Infrastructure.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<Web.Users.User>
    {
        public void Configure(EntityTypeBuilder<Web.Users.User> builder)
        {
            builder.ToTable("users");
            
            // Primary key
            builder.HasKey(u => u.Id);
            
            // Optimistic concurrency with xmin (PostgreSQL system column)
            builder.Property<uint>("xmin").IsRowVersion();
            
            // Scalar properties with snake_case naming
            builder.Property(u => u.Id)
                .HasColumnName("id")
                .IsRequired();
            
            builder.Property(u => u.Username)
                .HasColumnName("username")
                .IsRequired();
            
            builder.Property(u => u.Password)
                .HasColumnName("password")
                .IsRequired();
            
            builder.Property(u => u.Salt)
                .HasColumnName("salt")
                .IsRequired();
            
            builder.Property(u => u.Email)
                .HasColumnName("email")
                .IsRequired();
            
            builder.Property(u => u.DiscmanPoints)
                .HasColumnName("discman_points")
                .IsRequired();
            
            builder.Property(u => u.Elo)
                .HasColumnName("elo")
                .IsRequired();
            
            builder.Property(u => u.SimpleScoring)
                .HasColumnName("simple_scoring")
                .IsRequired();
            
            builder.Property(u => u.Emoji)
                .HasColumnName("emoji");
            
            builder.Property(u => u.Country)
                .HasColumnName("country");
            
            builder.Property(u => u.RegisterPutDistance)
                .HasColumnName("register_put_distance")
                .IsRequired();
            
            builder.Property(u => u.SettingsInitialized)
                .HasColumnName("settings_initialized")
                .IsRequired();
            
            builder.Property(u => u.LastEmailSent)
                .HasColumnName("last_email_sent");
            
            // PostgreSQL array columns
            builder.Property(u => u.Friends)
                .HasColumnName("friends")
                .HasColumnType("text[]")
                .IsRequired();
            
            builder.Property(u => u.NewsIdsSeen)
                .HasColumnName("news_ids_seen")
                .HasColumnType("text[]")
                .IsRequired();
            
            builder.Ignore(u => u.Achievements);
            
            // RatingHistory as separate table
            builder.OwnsMany(u => u.RatingHistory, rating =>
            {
                rating.ToTable("user_ratings");
                rating.WithOwner().HasForeignKey("UserId");
                rating.Property<int>("Id");
                rating.HasKey("Id");
                
                rating.Property(r => r.Elo)
                    .HasColumnName("elo")
                    .IsRequired();
                rating.Property(r => r.DateTime)
                    .HasColumnName("date_time")
                    .IsRequired();
            });
        }
    }
}
