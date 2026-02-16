using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing.Api;
using Web.Testing.Seeding;

namespace Web.ApiTests.Tests;

[TestFixture]
public sealed class AuthApiTests
{
    [Test]
    public async Task UnauthorizedEndpointRequiresAuth()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);
        var status = await api.GetUnauthorizedAsync("/api/users?searchString=test");
        Assert.That(status, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CanRegisterAndLogin()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        var registerResult = await api.RegisterUserAsync(username, password);
        Assert.That(registerResult.Username, Is.EqualTo(username));
        Assert.That(registerResult.Token, Is.Not.Null.And.Not.Empty);

        var loginResult = await api.LoginAsync(username, password);
        Assert.That(loginResult.Username, Is.EqualTo(username));
        Assert.That(loginResult.Token, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task DuplicateRegistrationFails()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        await api.RegisterUserAsync(username, password);
        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await api.RegisterUserAsync(username, password);
        });
    }
}
