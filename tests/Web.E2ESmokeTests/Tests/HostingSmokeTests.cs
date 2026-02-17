using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Web.E2ESmokeTests.Tests;

[TestFixture]
[Category("E2E")]
public sealed class HostingSmokeTests : E2ETestBase
{
    [Test]
    [CancelAfter(60_000)]
    public async Task RootServesNewFrontend()
    {
        await Page.GotoAsync(ServerUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        var title = await Page.TitleAsync();

        Assert.That(title, Is.Not.Empty, "Root should serve a page with a title");

        var signInVisible = await Page.GetByText("Sign In").IsVisibleAsync();
        var landingVisible = await Page.GetByText("Discman").IsVisibleAsync();

        Assert.That(signInVisible || landingVisible, Is.True,
            "Root should serve the new frontend (Sign In or Discman branding)");
    }

    [Test]
    [CancelAfter(60_000)]
    public async Task ApiEndpointsStillWork()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"{ServerUrl}/api/courses?filter=&latitude=0&longitude=0");

        Assert.That((int)response.StatusCode, Is.LessThan(500),
            "API endpoints should not return 5xx");
    }
}
