using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Users.Domain;

namespace Web.Infrastructure.EntityConfigurations
{
    public class ResetPasswordRequestConfiguration : IEntityTypeConfiguration<ResetPasswordRequest>
    {
        public void Configure(EntityTypeBuilder<ResetPasswordRequest> builder)
        {
            builder.ToTable("reset_password_requests");
            builder.HasKey(x => x.Id);
            
            // Index on Email for password reset lookup
            builder.HasIndex(x => x.Email);
            
            // All properties with snake_case naming
            builder.Property(x => x.Email)
                .HasColumnName("email");
            
            builder.Property(x => x.Username)
                .HasColumnName("username");
            
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");
        }
    }
}
