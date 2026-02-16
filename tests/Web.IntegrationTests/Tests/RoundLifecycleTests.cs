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
public sealed class RoundLifecycleTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Test]
    public async Task FullRoundLifecycle_StartScoreComplete()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"int{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var course = await api.CreateCourseAsync(login.Token, $"Course{Guid.NewGuid():N}"[..12], "Layout A", 9);

        var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { username });
        Assert.That(round.IsCompleted, Is.False);

        for (var hole = 0; hole < 9; hole++)
        {
            await api.UpdateScoreAsync(login.Token, round.Id, hole, 3, username);
        }

        var midRound = await api.GetRoundAsync(login.Token, round.Id);
        Assert.That(midRound.IsCompleted, Is.False);
        Assert.That(midRound.PlayerScores, Has.Count.EqualTo(1));
        Assert.That(midRound.PlayerScores[0].PlayerName, Is.EqualTo(username));

        await api.CompleteRoundAsync(login.Token, round.Id);

        var completed = await api.GetRoundAsync(login.Token, round.Id);
        Assert.That(completed.IsCompleted, Is.True);
    }

    [Test]
    public async Task MultiPlayerRound_AllPlayersScoreAndComplete()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var player1 = $"int{Guid.NewGuid():N}"[..12];
        var player2 = $"int{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        await api.RegisterUserAsync(player1, password);
        await api.RegisterUserAsync(player2, password);

        var login1 = await api.LoginAsync(player1, password);
        var login2 = await api.LoginAsync(player2, password);

        var course = await api.CreateCourseAsync(login1.Token, $"Course{Guid.NewGuid():N}"[..12], "Layout A", 9);
        var round = await api.StartRoundAsync(login1.Token, course.Id, new List<string> { player1, player2 });

        for (var hole = 0; hole < 9; hole++)
        {
            await api.UpdateScoreAsync(login1.Token, round.Id, hole, 3, player1);
            await api.UpdateScoreAsync(login2.Token, round.Id, hole, 4, player2);
        }

        await api.CompleteRoundAsync(login1.Token, round.Id);
        await api.CompleteRoundAsync(login2.Token, round.Id);

        var completed = await api.GetRoundAsync(login1.Token, round.Id);
        Assert.That(completed.IsCompleted, Is.True);
        Assert.That(completed.PlayerScores, Has.Count.EqualTo(2));
    }
}
