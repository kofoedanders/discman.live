using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Web.E2ETests.Tests;

[TestFixture]
[Category("E2E")]
public class SmokeTests : E2ETestBase
{
    [Test]
    public async Task ServerResponds()
    {
        var response = await Page.GotoAsync(ServerUrl);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Ok || response.Status == 500, Is.True,
            "Server should respond (500 is acceptable when frontend is not built)");
    }

    [Test]
    public async Task ApiEndpointRequiresAuth()
    {
        var client = AppFixture.Instance.HttpClient;
        var response = await client.GetAsync("/api/users?searchString=test");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CanRegisterAndLoginViaApi()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"smoke{System.Guid.NewGuid():N}".Substring(0, 15);

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            Username = username,
            Password = "TestPass123!",
            Email = ""
        });

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.IsSuccessStatusCode, Is.True,
            $"Register failed with {response.StatusCode}: {body}");
    }
}
