using System;
using Npgsql;
using NpgsqlTypes;
using NServiceBus;

namespace Web.Infrastructure
{
    public static class NServiceBusConfiguration
    {
        public static EndpointConfiguration ConfigureEndpoint()
        {
            var endpointConfiguration = new EndpointConfiguration("discman.web");
            
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology(QueueType.Quorum);
            transport.ConnectionString(Environment.GetEnvironmentVariable("DOTNET_RABBITMQ_CON_STRING"));

            var connectionString = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_CON_STRING");
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
            dialect.JsonBParameterModifier(parameter =>
            {
                var npgsqlParameter = (NpgsqlParameter)parameter;
                npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            });
            persistence.ConnectionBuilder(() => new NpgsqlConnection(connectionString));

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);
            
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();
            
            return endpointConfiguration;
        }
    }
}