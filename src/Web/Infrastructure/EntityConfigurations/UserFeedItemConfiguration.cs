using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Feeds.Domain;

namespace Web.Infrastructure.EntityConfigurations
{
    public class UserFeedItemConfiguration : IEntityTypeConfiguration<UserFeedItem>
    {
        public void Configure(EntityTypeBuilder<UserFeedItem> builder)
        {
            builder.ToTable("user_feed_items");
            builder.HasKey(x => x.Id);
            
            // Indexes for user feed queries
            builder.HasIndex(x => x.Username);
            builder.HasIndex(x => x.RegisteredAt);
            
            // All properties with snake_case naming
            builder.Property(x => x.Username)
                .HasColumnName("username");
            
            builder.Property(x => x.FeedItemId)
                .HasColumnName("feed_item_id");
            
            builder.Property(x => x.ItemType)
                .HasColumnName("item_type");
            
            builder.Property(x => x.RegisteredAt)
                .HasColumnName("registered_at");
        }
    }
}
