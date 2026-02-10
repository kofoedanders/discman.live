using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ETests.Helpers;
using Web.Users.Queries;

namespace Web.E2ETests.Tests;

[TestFixture]
[Category("E2E")]
public class YearSummaryTests : E2ETestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Test]
    public async Task YearSummary_WithNoRounds_ReturnsZeroedSummary()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var year = DateTime.UtcNow.Year;

        await ApiHelper.RegisterUserAsync(client, username, password);
        var loginResponse = await ApiHelper.LoginAsync(client, username, password);

        using var authClient = ApiHelper.CreateAuthenticatedClient(loginResponse.Token);
        var response = await authClient.GetAsync($"/api/users/{username}/yearsummary/{year}");

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
    public async Task YearSummary_WithRoundsButNoQualifyingCardmates_ReturnsValidSummary()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var cardmate = $"e2e{Guid.NewGuid():N}"[..12];
        var cardmatePassword = "TestPass123!";
        var year = DateTime.UtcNow.Year;

        await ApiHelper.RegisterUserAsync(client, username, password);
        await ApiHelper.RegisterUserAsync(client, cardmate, cardmatePassword);

        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
        var cardmateLogin = await ApiHelper.LoginAsync(client, cardmate, cardmatePassword);

        var courseResponse = await ApiHelper.CreateCourseAsync(
            client,
            loginResponse.Token,
            $"Course {username}",
            "Layout A",
            9);

        var roundResponse = await ApiHelper.StartRoundAsync(
            client,
            loginResponse.Token,
            courseResponse.Id,
            new List<string> { username, cardmate });

        for (var holeIndex = 0; holeIndex < 9; holeIndex += 1)
        {
            await ApiHelper.UpdateScoreAsync(
                client,
                loginResponse.Token,
                roundResponse.Id,
                holeIndex,
                3,
                username);

            await ApiHelper.UpdateScoreAsync(
                client,
                cardmateLogin.Token,
                roundResponse.Id,
                holeIndex,
                3,
                cardmate);
        }

        await ApiHelper.CompleteRoundAsync(client, loginResponse.Token, roundResponse.Id);
        await ApiHelper.CompleteRoundAsync(client, cardmateLogin.Token, roundResponse.Id);

        using var authClient = ApiHelper.CreateAuthenticatedClient(loginResponse.Token);
        var response = await authClient.GetAsync($"/api/users/{username}/yearsummary/{year}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadAsStringAsync();
        var summary = JsonSerializer.Deserialize<UserYearSummary>(payload, JsonOptions);

        Assert.That(summary, Is.Not.Null);
        Assert.That(summary!.RoundsPlayed, Is.EqualTo(1));
        // TotalScore is relative to par, so scoring par on every hole gives 0
        Assert.That(summary.TotalScore, Is.EqualTo(0));
        Assert.That(summary.BestCardmate, Is.Null.Or.Empty);
        Assert.That(summary.WorstCardmate, Is.Null.Or.Empty);
        Assert.That(summary.MostPlayedCourse, Is.EqualTo(courseResponse.Name));
    }

    [Test]
    public async Task YearSummary_WithMultipleRounds_ReturnsCorrectStats()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var cardmate = $"e2e{Guid.NewGuid():N}"[..12];
        var cardmatePassword = "TestPass123!";
        var year = DateTime.UtcNow.Year;

        await ApiHelper.RegisterUserAsync(client, username, password);
        await ApiHelper.RegisterUserAsync(client, cardmate, cardmatePassword);

        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
        var cardmateLogin = await ApiHelper.LoginAsync(client, cardmate, cardmatePassword);

        var courseResponse = await ApiHelper.CreateCourseAsync(
            client,
            loginResponse.Token,
            $"Course {username}",
            "Layout A",
            9);

        for (var roundIndex = 0; roundIndex < 5; roundIndex += 1)
        {
            var roundResponse = await ApiHelper.StartRoundAsync(
                client,
                loginResponse.Token,
                courseResponse.Id,
                new List<string> { username, cardmate });

            for (var holeIndex = 0; holeIndex < 9; holeIndex += 1)
            {
                await ApiHelper.UpdateScoreAsync(
                    client,
                    loginResponse.Token,
                    roundResponse.Id,
                    holeIndex,
                    3,
                    username);

                await ApiHelper.UpdateScoreAsync(
                    client,
                    cardmateLogin.Token,
                    roundResponse.Id,
                    holeIndex,
                    3,
                    cardmate);
            }

            await ApiHelper.CompleteRoundAsync(client, loginResponse.Token, roundResponse.Id);
            await ApiHelper.CompleteRoundAsync(client, cardmateLogin.Token, roundResponse.Id);
        }

        using var authClient = ApiHelper.CreateAuthenticatedClient(loginResponse.Token);
        var response = await authClient.GetAsync($"/api/users/{username}/yearsummary/{year}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadAsStringAsync();
        var summary = JsonSerializer.Deserialize<UserYearSummary>(payload, JsonOptions);

        Assert.That(summary, Is.Not.Null);
        Assert.That(summary!.RoundsPlayed, Is.GreaterThanOrEqualTo(5));
        Assert.That(summary.BestCardmate, Is.Not.Null.And.Not.Empty);
        Assert.That(summary.WorstCardmate, Is.Not.Null.And.Not.Empty);
        Assert.That(summary.MostPlayedCourse, Is.EqualTo(courseResponse.Name));
        Assert.That(summary.MostPlayedCourseRoundsCount, Is.GreaterThanOrEqualTo(5));
    }
}
