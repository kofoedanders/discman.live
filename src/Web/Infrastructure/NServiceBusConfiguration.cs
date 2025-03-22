using System;
using NServiceBus;

namespace Web.Infrastructure
{
    public static class NServiceBusConfiguration
    {
        public static EndpointConfiguration ConfigureEndpoint()
        {
            var endpointConfiguration = new EndpointConfiguration("discman.web");
            
            // Updated for NServiceBus 8.x
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology(QueueType.Quorum);
            transport.ConnectionString(Environment.GetEnvironmentVariable("DOTNET_RABBITMQ_CON_STRING"));

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);
            
            // Recommended for NServiceBus 8.x
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();
            
            return endpointConfiguration;
        }
    }
}