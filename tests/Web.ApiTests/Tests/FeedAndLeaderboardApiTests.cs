using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing.Api;

namespace Web.ApiTests.Tests;

[TestFixture]
public sealed class FeedAndLeaderboardApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Test]
    public async Task FeedReturnsOkAfterCompletedRound()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
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

        var feedResponse = await api.GetFeedsRawAsync(login.Token);
        Assert.That(feedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var feedBody = await feedResponse.Content.ReadAsStringAsync();
        Assert.That(feedBody, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task LeaderboardReturnsOk()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
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

        var leaderboardResponse = await api.GetLeaderboardRawAsync(login.Token);
        Assert.That(leaderboardResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await leaderboardResponse.Content.ReadAsStringAsync();
        Assert.That(body, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task FeedRequiresAuthentication()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);
        var status = await api.GetUnauthorizedAsync("/api/feeds?pageNumber=1&pageSize=10&itemType=");
        Assert.That(status, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task LeaderboardRequiresAuthentication()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);
        var status = await api.GetUnauthorizedAsync("/api/leaderboard?onlyFriends=false&month=0");
        Assert.That(status, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
