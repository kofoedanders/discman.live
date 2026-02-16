using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing.Api;
using Web.Users.Queries;

namespace Web.ApiTests.Tests;

[TestFixture]
public sealed class YearSummaryApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Test]
    public async Task YearSummary_WithNoRounds_ReturnsZeroedSummary()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var year = DateTime.UtcNow.Year;

        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var response = await api.GetYearSummaryRawAsync(login.Token, username, year);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadAsStringAsync();
        var summary = JsonSerializer.Deserialize<UserYearSummary>(payload, JsonOptions);

        Assert.That(summary, Is.Not.Null);
        Assert.That(summary!.RoundsPlayed, Is.EqualTo(0));
        Assert.That(summary.HoursPlayed, Is.EqualTo(0));
        Assert.That(summary.TotalScore, Is.EqualTo(0));
        Assert.That(summary.BestCardmate, Is.Null.Or.Empty);
        Assert.That(summary.WorstCardmate, Is.Null.Or.Empty);
        Assert.That(summary.MostPlayedCourse, Is.Null.Or.Empty);
    }

    [Test]
    public async Task YearSummary_WithSingleRound_ReturnsValidSummary()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var cardmate = $"api{Guid.NewGuid():N}"[..12];
        var year = DateTime.UtcNow.Year;

        await api.RegisterUserAsync(username, password);
        await api.RegisterUserAsync(cardmate, password);

        var login = await api.LoginAsync(username, password);
        var cardmateLogin = await api.LoginAsync(cardmate, password);

        var course = await api.CreateCourseAsync(login.Token, $"Course{Guid.NewGuid():N}"[..12], "Layout A", 9);
        var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { username, cardmate });

        for (var hole = 0; hole < 9; hole++)
        {
            await api.UpdateScoreAsync(login.Token, round.Id, hole, 3, username);
            await api.UpdateScoreAsync(cardmateLogin.Token, round.Id, hole, 3, cardmate);
        }

        await api.CompleteRoundAsync(login.Token, round.Id);
        await api.CompleteRoundAsync(cardmateLogin.Token, round.Id);

        var response = await api.GetYearSummaryRawAsync(login.Token, username, year);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadAsStringAsync();
        var summary = JsonSerializer.Deserialize<UserYearSummary>(payload, JsonOptions);

        Assert.That(summary, Is.Not.Null);
        Assert.That(summary!.RoundsPlayed, Is.EqualTo(1));
        Assert.That(summary.TotalScore, Is.EqualTo(0));
        Assert.That(summary.MostPlayedCourse, Is.EqualTo(course.Name));
    }

    [Test]
    public async Task YearSummary_WithMultipleRounds_ReturnsCorrectStats()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var cardmate = $"api{Guid.NewGuid():N}"[..12];
        var year = DateTime.UtcNow.Year;

        await api.RegisterUserAsync(username, password);
        await api.RegisterUserAsync(cardmate, password);

        var login = await api.LoginAsync(username, password);
        var cardmateLogin = await api.LoginAsync(cardmate, password);

        var course = await api.CreateCourseAsync(login.Token, $"Course{Guid.NewGuid():N}"[..12], "Layout A", 9);

        for (var roundIndex = 0; roundIndex < 5; roundIndex++)
        {
            var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { username, cardmate });

            for (var hole = 0; hole < 9; hole++)
            {
                await api.UpdateScoreAsync(login.Token, round.Id, hole, 3, username);
                await api.UpdateScoreAsync(cardmateLogin.Token, round.Id, hole, 3, cardmate);
            }

            await api.CompleteRoundAsync(login.Token, round.Id);
            await api.CompleteRoundAsync(cardmateLogin.Token, round.Id);
        }

        var response = await api.GetYearSummaryRawAsync(login.Token, username, year);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadAsStringAsync();
        var summary = JsonSerializer.Deserialize<UserYearSummary>(payload, JsonOptions);

        Assert.That(summary, Is.Not.Null);
        Assert.That(summary!.RoundsPlayed, Is.GreaterThanOrEqualTo(5));
        Assert.That(summary.BestCardmate, Is.Not.Null.And.Not.Empty);
        Assert.That(summary.WorstCardmate, Is.Not.Null.And.Not.Empty);
        Assert.That(summary.MostPlayedCourse, Is.EqualTo(course.Name));
        Assert.That(summary.MostPlayedCourseRoundsCount, Is.GreaterThanOrEqualTo(5));
    }
}
