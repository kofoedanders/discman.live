using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Courses;

namespace Web.Infrastructure.EntityConfigurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("courses");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Id)
                .HasColumnName("id")
                .IsRequired();
            builder.Property(c => c.Name)
                .HasColumnName("name")
                .IsRequired();
            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            builder.Property(c => c.Layout)
                .HasColumnName("layout");
            builder.Property(c => c.Country)
                .HasColumnName("country");
            
            builder.Property(c => c.Admins)
                .HasColumnName("admins")
                .HasColumnType("text[]")
                .IsRequired();
            
            builder.OwnsOne(c => c.Coordinates, coord =>
            {
                coord.Property(x => x.Latitude)
                    .HasColumnName("latitude")
                    .HasPrecision(9, 6)
                    .IsRequired();
                coord.Property(x => x.Longitude)
                    .HasColumnName("longitude")
                    .HasPrecision(9, 6)
                    .IsRequired();
            });
            
            builder.OwnsMany(c => c.Holes, hole =>
            {
                hole.ToTable("course_holes");
                hole.WithOwner().HasForeignKey("CourseId");
                hole.Property<int>("Id");
                hole.HasKey("Id");
                
                hole.Property(h => h.Number)
                    .HasColumnName("number")
                    .IsRequired();
                hole.Property(h => h.Par)
                    .HasColumnName("par")
                    .IsRequired();
                hole.Property(h => h.Distance)
                    .HasColumnName("distance")
                    .IsRequired();
                hole.Property(h => h.Average)
                    .HasColumnName("average")
                    .IsRequired();
                hole.Property(h => h.Rating)
                    .HasColumnName("rating")
                    .IsRequired();
            });
            
            builder.Ignore(c => c.CourseAverageScore);
        }
    }
}
