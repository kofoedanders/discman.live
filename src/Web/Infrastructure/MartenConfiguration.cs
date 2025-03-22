using System;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weasel.Postgresql;
using Web.Feeds.Domain;

namespace Web.Infrastructure
{
    public static class MartenConfiguration
    {
        public static void ConfigureMarten(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            var constring = configuration.GetValue<string>("POSTGRES_CON_STRING");
            Console.WriteLine(constring);
            var store = DocumentStore.For(_ =>
            {
                _.DatabaseSchemaName = $"disclive_{hostEnvironment.EnvironmentName}";
                _.Connection(configuration.GetValue<string>("POSTGRES_CON_STRING"));

                _.CreateDatabasesForTenants(c =>
                {
                    c.ForTenant()
                        .CheckAgainstPgDatabase()
                        .WithOwner("postgres")
                        .WithEncoding("UTF-8")
                        .ConnectionLimit(-1);
                });

                _.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;

                _.Schema.For<UserFeedItem>().Index(x => x.Username);
                _.Schema.For<GlobalFeedItem>().Index(x => x.Id);
            });

            services.AddSingleton<IDocumentStore>(store);
            services.AddScoped(sp => sp.GetService<IDocumentStore>().OpenSession());
        }
    }
}