using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Web.Infrastructure
{
    /// <summary>
    /// Design-time factory for EF Core migrations.
    /// Uses hardcoded localhost connection string for migrations only.
    /// </summary>
    public class DiscmanDbContextFactory : IDesignTimeDbContextFactory<DiscmanDbContext>
    {
        public DiscmanDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DiscmanDbContext>();
            
            // Design-time connection string (for migrations only)
            // Runtime uses DOTNET_POSTGRES_CON_STRING from environment
            optionsBuilder.UseNpgsql("host=localhost;database=discman;username=postgres;password=Password12!");
            
            return new DiscmanDbContext(optionsBuilder.Options);
        }
    }
}
