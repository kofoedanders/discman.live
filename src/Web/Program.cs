using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NpgsqlTypes;
using NServiceBus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Web.Infrastructure;
using EnvironmentName = Microsoft.Extensions.Hosting.EnvironmentName;

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
                CreateHostBuilder(args).Build().Run();
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

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var enableElk = Environment.GetEnvironmentVariable("DOTNET_ENABLE_ELK");
            var isDevelopment = environment == Environments.Development;
            if (!isDevelopment && enableElk is not null && enableElk == "true")
            {
                logConfig.WriteTo.Http("http://logstash:7000");
            }

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

            // logConfig.WriteTo.PostgreSQL(Environment.GetEnvironmentVariable("DOTNET_POSTGRES_CON_STRING"), tableName, columnWriters, needAutoCreateTable: true);
            //
            Log.Logger = logConfig.CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseSerilog()
                .UseNServiceBus(context => NServiceBusConfiguration.ConfigureEndpoint());
    }
}