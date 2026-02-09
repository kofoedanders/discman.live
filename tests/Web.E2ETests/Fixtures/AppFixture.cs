using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Web.Courses;
using Web.Rounds;
using Web.Users;

// Root namespace so [SetUpFixture] applies to ALL test classes in the assembly
namespace Web.E2ETests;

[NUnit.Framework.SetUpFixture]
public class AppFixture
{
    private PostgreSqlContainer _postgresContainer = null!;
    private RabbitMqContainer _rabbitMqContainer = null!;
    private DiscmanWebApplicationFactory _factory = null!;

    public static AppFixture Instance { get; private set; } = null!;
    public string ServerUrl { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;
    public string PostgresConnectionString { get; private set; } = null!;

    [NUnit.Framework.OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Instance = this;

        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("disclive")
            .WithUsername("postgres")
            .WithPassword("testpassword")
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        );

        PostgresConnectionString = _postgresContainer.GetConnectionString();
        var rabbitConnectionString = _rabbitMqContainer.GetConnectionString();

        Environment.SetEnvironmentVariable("DOTNET_RABBITMQ_CON_STRING", rabbitConnectionString);
        Environment.SetEnvironmentVariable("DOTNET_POSTGRES_CON_STRING", PostgresConnectionString);
        Environment.SetEnvironmentVariable("DOTNET_TOKEN_SECRET", "E2E_TEST_TOKEN_SECRET_THAT_IS_LONG_ENOUGH_FOR_HMAC");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        _factory = new DiscmanWebApplicationFactory(PostgresConnectionString);

        HttpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        ServerUrl = _factory.ServerUrl
            ?? throw new InvalidOperationException("Kestrel host did not start. ServerUrl is null.");

        Console.WriteLine($"[AppFixture] App started at {ServerUrl}");
    }

    [NUnit.Framework.OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        Serilog.Log.CloseAndFlush();
        HttpClient?.Dispose();
        _factory?.Dispose();

        await Task.WhenAll(
            _postgresContainer.DisposeAsync().AsTask(),
            _rabbitMqContainer.DisposeAsync().AsTask()
        );

        Environment.SetEnvironmentVariable("DOTNET_RABBITMQ_CON_STRING", null);
        Environment.SetEnvironmentVariable("DOTNET_POSTGRES_CON_STRING", null);
        Environment.SetEnvironmentVariable("DOTNET_TOKEN_SECRET", null);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
}

// CompositeHost pattern for .NET 9: https://medium.com/younited-tech-blog/end-to-end-test-a-blazor-app-with-playwright-part-3-48c0edeff4b6
public class DiscmanWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;

    public string? ServerUrl { get; private set; }

    public DiscmanWebApplicationFactory(string postgresConnectionString)
    {
        _postgresConnectionString = postgresConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("POSTGRES_CON_STRING", _postgresConnectionString);
        builder.UseSetting("TOKEN_SECRET", "E2E_TEST_TOKEN_SECRET_THAT_IS_LONG_ENOUGH_FOR_HMAC");

        builder.ConfigureServices(services =>
        {
            // Remove background workers that fire immediately and crash the process
            // with unhandled Npgsql exceptions. Keep NServiceBus hosted services.
            var workerTypes = new[]
            {
                typeof(UpdateCourseRatingsWorker),
                typeof(UpdateInActiveRoundsWorker),
                typeof(ResetPasswordWorker),
                typeof(UserEmailNotificationWorker)
            };
            foreach (var workerType in workerTypes)
            {
                services.RemoveAll(workerType);
                var descriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(IHostedService) &&
                    d.ImplementationType == workerType);
                if (descriptor != null) services.Remove(descriptor);
            }

            // Override SPA static files root to serve the pre-built React app.
            // Startup.cs configures RootPath = "wwwroot" which is empty in dev;
            // the CRA build output lives in ClientApp/build.
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = base.CreateHost(builder);

        // Use TaskCompletionSource to reliably capture the Kestrel address
        // after the host has fully started (ApplicationStarted event)
        var addressReady = new TaskCompletionSource<string[]>();

        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls("http://127.0.0.1:0");
            webHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(provider =>
                {
                    var lifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    var server = provider.GetRequiredService<IServer>();
                    lifetime.ApplicationStarted.Register(() =>
                    {
                        var addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses.ToArray();
                        addressReady.SetResult(addresses);
                    });
                    return addressReady;
                });
            });
        });

        var kestrelHost = builder.Build();
        kestrelHost.Start();

        kestrelHost.Services.GetRequiredService<TaskCompletionSource<string[]>>();
        var addresses = addressReady.Task.GetAwaiter().GetResult();
        ServerUrl = addresses.First();

        return new CompositeHost(testHost, kestrelHost);
    }
}

internal sealed class CompositeHost : IHost
{
    private readonly IHost _testHost;
    private readonly IHost _kestrelHost;

    public CompositeHost(IHost testHost, IHost kestrelHost)
    {
        _testHost = testHost;
        _kestrelHost = kestrelHost;
    }

    public IServiceProvider Services => _testHost.Services;

    public void Dispose()
    {
        _testHost.Dispose();
        _kestrelHost.Dispose();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _testHost.StartAsync(cancellationToken);
        await _kestrelHost.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _testHost.StopAsync(cancellationToken);
        await _kestrelHost.StopAsync(cancellationToken);
    }
}
