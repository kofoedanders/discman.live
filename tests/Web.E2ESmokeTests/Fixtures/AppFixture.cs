using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing;

namespace Web.E2ESmokeTests;

[SetUpFixture]
public sealed class AppFixture
{
    public static TestHostFixture Instance { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var externalBaseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL");
        Instance = new TestHostFixture(externalBaseUrl);
        await Instance.StartAsync();

        if (string.IsNullOrWhiteSpace(externalBaseUrl))
        {
            Console.WriteLine($"[E2ESmoke] App started at {Instance.ServerUrl}");
        }
        else
        {
            Console.WriteLine($"[E2ESmoke] Using deployed app at {Instance.ServerUrl}");
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Instance.DisposeAsync();
    }
}
