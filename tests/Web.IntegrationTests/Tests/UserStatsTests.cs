using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing.Api;

namespace Web.IntegrationTests.Tests;

[TestFixture]
public sealed class UserStatsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Test]
    public async Task UserStatsEndpointReturnsOk()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"int{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/users/{username}/stats?since=2020-01-01&includeMonths=12");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UserAchievementsEndpointReturnsOk()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"int{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{username}/achievements");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UserStatsAfterCompletedRound()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"int{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var course = await api.CreateCourseAsync(login.Token, $"Course{Guid.NewGuid():N}"[..12], "Layout A", 9);
        var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { username });

        for (var hole = 0; hole < 9; hole++)
        {
            await api.UpdateScoreAsync(login.Token, round.Id, hole, 3, username);
        }

        await api.CompleteRoundAsync(login.Token, round.Id);

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"/api/users/{username}/stats?since=2020-01-01&includeMonths=120");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.Not.Null.And.Not.Empty);
    }
}
