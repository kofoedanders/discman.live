using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ESmokeTests.PageObjects;
using Web.Testing.Api;
using Web.Testing.Seeding;

namespace Web.E2ESmokeTests.Tests;

[TestFixture]
[Category("E2E")]
public sealed class SmokeFlowTests : E2ETestBase
{
    [Test]
    [CancelAfter(120_000)]
    public async Task LoginCreateRoundRegisterScore()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        await TestUsers.EnsureSeedUserExistsAsync(api);
        var login = await api.LoginAsync(TestUsers.SeededUsername, TestUsers.SeededPassword);

        var course = await api.CreateCourseAsync(login.Token, $"Course {Guid.NewGuid():N}"[..12], "Layout A", 9);
        var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { login.Username });
        await api.UpdateScoreAsync(login.Token, round.Id, holeIndex: 0, strokes: 3, username: login.Username);

        using var probe = new HttpClient();
        var response = await probe.GetAsync($"{ServerUrl}/login");
        TestContext.Out.WriteLine($"Probe GET {ServerUrl}/login => {response.StatusCode}");
        Assert.That((int)response.StatusCode, Is.LessThan(500), "Server returned 5xx before Playwright navigation");

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(TestUsers.SeededUsername, TestUsers.SeededPassword);

        var homePage = new HomePage(Page);
        await homePage.WaitForFeedAsync();

        var roundPage = new RoundPage(Page, ServerUrl);
        await roundPage.NavigateToAsync(round.Id);
        await roundPage.WaitForRoundContentAsync();
    }
}
