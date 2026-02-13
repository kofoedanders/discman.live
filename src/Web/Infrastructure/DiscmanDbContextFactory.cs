using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

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
            var connectionString = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_CON_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "host=localhost;database=discman;username=postgres;password=Password12!";
            }

            optionsBuilder.UseNpgsql(connectionString);
            
            return new DiscmanDbContext(optionsBuilder.Options);
        }
    }
}
