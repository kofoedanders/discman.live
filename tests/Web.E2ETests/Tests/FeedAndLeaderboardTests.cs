using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ETests.Helpers;
using Web.E2ETests.PageObjects;

namespace Web.E2ETests.Tests;

[TestFixture]
[Category("E2E")]
public class FeedAndLeaderboardTests : E2ETestBase
{
    [Test]
    public async Task FeedShowsCompletedRound()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var courseName = $"Course {username}";

        await ApiHelper.RegisterUserAsync(client, username, password);
        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
        var courseResponse = await ApiHelper.CreateCourseAsync(
            client,
            loginResponse.Token,
            courseName,
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

        await ApiHelper.CompleteRoundAsync(client, loginResponse.Token, roundResponse.Id);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();

        await Page.Locator("article.media").First.WaitForAsync();
        Assert.That(
            await Page.Locator($".box:has-text('{courseName}')").First.IsVisibleAsync(),
            Is.True);
    }

    [Test]
    public async Task CanLikeFeedItem()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var courseName = $"Course {username}";

        await ApiHelper.RegisterUserAsync(client, username, password);
        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
        var courseResponse = await ApiHelper.CreateCourseAsync(
            client,
            loginResponse.Token,
            courseName,
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

        await ApiHelper.CompleteRoundAsync(client, loginResponse.Token, roundResponse.Id);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();

        // Dismiss any active modals by removing them via JS (clicking background doesn't reliably close them)
        await Page.EvaluateAsync("document.querySelectorAll('.modal.is-active').forEach(m => m.classList.remove('is-active'))");

        await Page.Locator("article.media").First.WaitForAsync();
        await Page.Locator(".fa-thumbs-up").First.EvaluateAsync("el => el.click()");

        await Page.Locator(".badge:has-text('1')").First.WaitForAsync();
        Assert.That(await Page.Locator(".badge:has-text('1')").First.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task LeaderboardDisplaysAfterLogin()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var courseName = $"Course {username}";

        await ApiHelper.RegisterUserAsync(client, username, password);
        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
        var courseResponse = await ApiHelper.CreateCourseAsync(
            client,
            loginResponse.Token,
            courseName,
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

        await ApiHelper.CompleteRoundAsync(client, loginResponse.Token, roundResponse.Id);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();

        await Page.GotoAsync($"{ServerUrl}/leaders");

        await Page.Locator("table").First.WaitForAsync();
        Assert.That(await Page.Locator("table").First.IsVisibleAsync(), Is.True);
        Assert.That(await Page.Locator("thead td:text-is('Player')").IsVisibleAsync(), Is.True);
        Assert.That(await Page.Locator("thead td:text-is('Rounds')").IsVisibleAsync(), Is.True);
    }
}
