using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Tournaments.Domain;

namespace Web.Infrastructure.EntityConfigurations
{
    public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
    {
        public void Configure(EntityTypeBuilder<Tournament> builder)
        {
            builder.ToTable("tournaments");
            builder.HasKey(t => t.Id);
            
            builder.Property(t => t.Id)
                .HasColumnName("id")
                .IsRequired();
            
            builder.Property(t => t.Name)
                .HasColumnName("name")
                .IsRequired();
            
            builder.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            
            builder.Property(t => t.Start)
                .HasColumnName("start")
                .IsRequired();
            
            builder.Property(t => t.End)
                .HasColumnName("end")
                .IsRequired();
            
            builder.Property(t => t.Players)
                .HasColumnType("text[]")
                .HasColumnName("players")
                .IsRequired();
            
            builder.Property(t => t.Admins)
                .HasColumnType("text[]")
                .HasColumnName("admins")
                .IsRequired();
            
            builder.Property(t => t.Courses)
                .HasColumnType("uuid[]")
                .HasColumnName("courses")
                .IsRequired();
            
            builder.Property(t => t.Prices)
                .HasColumnType("jsonb")
                .HasColumnName("prices");
        }
    }
}
