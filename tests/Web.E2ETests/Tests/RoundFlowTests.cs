using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ETests.Helpers;
using Web.E2ETests.PageObjects;

namespace Web.E2ETests.Tests;

[TestFixture]
[Category("E2E")]
public class RoundFlowTests : E2ETestBase
{
    [Test]
    public async Task CanLoginCreateRoundAndCompleteRound()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        // 1. Seed data via API: user, course, round, scores
        await ApiHelper.RegisterUserAsync(client, username, password);
        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
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
            new List<string> { username });

        for (var holeIndex = 0; holeIndex < 9; holeIndex += 1)
        {
            await ApiHelper.UpdateScoreAsync(
                client,
                loginResponse.Token,
                roundResponse.Id,
                holeIndex,
                3,
                username);
        }

        // 2. Login via UI and verify home page
        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();
        Assert.That(await homePage.IsLoggedInAsync(), Is.True);

        // 3. Navigate to round and verify scorecard renders
        var roundPage = new RoundPage(Page, ServerUrl);
        await roundPage.NavigateToAsync(roundResponse.Id);
        await roundPage.WaitForScoreCardAsync();

        // 4. Complete the round via API (sign for the only player)
        await ApiHelper.CompleteRoundAsync(client, loginResponse.Token, roundResponse.Id);

        // 5. Re-navigate so the SPA fetches the completed round from the server
        await roundPage.NavigateToAsync(roundResponse.Id);

        // 6. Verify the completed round summary is shown
        Assert.That(await roundPage.IsRoundCompletedAsync(), Is.True);
    }
}
