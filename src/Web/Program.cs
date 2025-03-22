using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NpgsqlTypes;
using NServiceBus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Web.Infrastructure;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureLogger();

            try
            {
                Log.Information("Starting up");
                
                var builder = WebApplication.CreateBuilder(args);
                
                // Add services to the container
                var startup = new Startup(builder.Configuration, builder.Environment);
                startup.ConfigureServices(builder.Services);
                
                // Configure NServiceBus
                builder.Host.UseNServiceBus(context => NServiceBusConfiguration.ConfigureEndpoint());
                
                // Configure Serilog
                builder.Host.UseSerilog();
                
                var app = builder.Build();
                
                // Configure the HTTP request pipeline
                startup.Configure(app, app.Environment);
                
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureLogger()
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Marten", LogEventLevel.Warning)
                .WriteTo.Console()
                .Enrich.WithProperty("ApplicationName", "discman.live")
                .MinimumLevel.Warning();

            // var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            // var enableElk = Environment.GetEnvironmentVariable("DOTNET_ENABLE_ELK");
            // var isDevelopment = environment == Environments.Development;
            // if (!isDevelopment && enableElk is not null && enableElk == "true")
            // {
            //     logConfig.WriteTo.Http("http://logstash:7000");
            // }

            string tableName = "discman_logs";

            var columnWriters = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                {"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
                {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
                {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                {"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
                {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
                {"machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
            };

            logConfig.WriteTo.PostgreSQL(Environment.GetEnvironmentVariable("DOTNET_POSTGRES_CON_STRING"), tableName, columnWriters, needAutoCreateTable: true);
            Log.Logger = logConfig.CreateLogger();
        }
    }
}