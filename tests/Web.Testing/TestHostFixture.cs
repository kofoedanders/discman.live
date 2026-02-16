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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Web.Courses;
using Web.Infrastructure;
using Web.Users;

namespace Web.Testing;

public sealed class TestHostFixture : IAsyncDisposable
{
    private PostgreSqlContainer? _postgresContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    private DiscmanWebApplicationFactory? _factory;

    public string ServerUrl { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;
    public string PostgresConnectionString { get; private set; } = null!;
    public IServiceProvider Services => _factory?.Services ?? throw new InvalidOperationException("Host not started");

    public async Task StartAsync()
    {
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
        Environment.SetEnvironmentVariable("DOTNET_TOKEN_SECRET", "TEST_TOKEN_SECRET_THAT_IS_LONG_ENOUGH_FOR_HMAC");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        _factory = new DiscmanWebApplicationFactory(PostgresConnectionString);

        HttpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        ServerUrl = _factory.ServerUrl
            ?? throw new InvalidOperationException("Kestrel host did not start. ServerUrl is null.");
    }

    public async ValueTask DisposeAsync()
    {
        Serilog.Log.CloseAndFlush();
        HttpClient?.Dispose();
        _factory?.Dispose();

        if (_postgresContainer != null && _rabbitMqContainer != null)
        {
            await Task.WhenAll(
                _postgresContainer.DisposeAsync().AsTask(),
                _rabbitMqContainer.DisposeAsync().AsTask()
            );
        }

        Environment.SetEnvironmentVariable("DOTNET_RABBITMQ_CON_STRING", null);
        Environment.SetEnvironmentVariable("DOTNET_POSTGRES_CON_STRING", null);
        Environment.SetEnvironmentVariable("DOTNET_TOKEN_SECRET", null);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
}

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
        builder.UseSetting("TOKEN_SECRET", "TEST_TOKEN_SECRET_THAT_IS_LONG_ENOUGH_FOR_HMAC");

        builder.ConfigureServices(services =>
        {
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

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = base.CreateHost(builder);

        using (var scope = testHost.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DiscmanDbContext>();
            dbContext.Database.Migrate();
        }

        var webProjectDir = FindWebProjectDirectory();

        var addressReady = new TaskCompletionSource<string[]>();

        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls("http://127.0.0.1:0");
            webHostBuilder.UseContentRoot(webProjectDir);
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

    private static string FindWebProjectDirectory()
    {
        var dir = Directory.GetCurrentDirectory();
        for (var i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, "src", "Web");
            if (File.Exists(Path.Combine(candidate, "Web.csproj")))
                return candidate;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        var assemblyDir = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
        return assemblyDir;
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

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            _testHost.StartAsync(cancellationToken),
            _kestrelHost.StartAsync(cancellationToken)
        );
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            _testHost.StopAsync(cancellationToken),
            _kestrelHost.StopAsync(cancellationToken)
        );
    }
}
