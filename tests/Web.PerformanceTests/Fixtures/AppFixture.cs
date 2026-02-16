using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing;

namespace Web.PerformanceTests;

[SetUpFixture]
public sealed class AppFixture
{
    public static TestHostFixture Instance { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Instance = new TestHostFixture();
        await Instance.StartAsync();
        Console.WriteLine($"[Perf] App started at {Instance.ServerUrl}");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Instance.DisposeAsync();
    }
}
