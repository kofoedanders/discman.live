using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing.Api;

namespace Web.ApiTests.Tests;

[TestFixture]
public sealed class RoundsApiTests
{
    [Test]
    public async Task CanCreateRoundAndRegisterScore()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var course = await api.CreateCourseAsync(login.Token, $"Course{Guid.NewGuid():N}"[..12], "Layout A", 9);
        var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { login.Username });
        await api.UpdateScoreAsync(login.Token, round.Id, holeIndex: 0, strokes: 3, username: login.Username);

        Assert.That(round.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task CanCompleteRoundAndVerify()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var course = await api.CreateCourseAsync(login.Token, $"Course{Guid.NewGuid():N}"[..12], "Layout A", 9);
        var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { login.Username });

        for (var hole = 0; hole < 9; hole++)
        {
            await api.UpdateScoreAsync(login.Token, round.Id, hole, 3, login.Username);
        }

        await api.CompleteRoundAsync(login.Token, round.Id);

        var completedRound = await api.GetRoundAsync(login.Token, round.Id);
        Assert.That(completedRound.IsCompleted, Is.True);
    }
}
