using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ESmokeTests.PageObjects;
using Web.Testing.Api;
using Web.Testing.Seeding;

namespace Web.E2ESmokeTests.Tests;

[TestFixture]
[Category("E2E")]
public sealed class ProfileTests : E2ETestBase
{
    [Test]
    [CancelAfter(120_000)]
    public async Task ProfilePageShowsUserDetailsAndTabs()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);
        await TestUsers.EnsureSeedUserExistsAsync(api);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(TestUsers.SeededUsername, TestUsers.SeededPassword);

        var profilePage = new ProfilePage(Page, ServerUrl);
        await profilePage.NavigateToAsync();
        await profilePage.WaitForPageAsync();

        var roundsTabVisible = await profilePage.IsRoundsTabVisibleAsync();
        Assert.That(roundsTabVisible, Is.True, "Profile should show Rounds tab");

        var achievementsTabVisible = await profilePage.IsAchievementsTabVisibleAsync();
        Assert.That(achievementsTabVisible, Is.True, "Profile should show Achievements tab");

        await profilePage.ClickAchievementsTabAsync();
        await Task.Delay(500);

        await profilePage.ClickRoundsTabAsync();
        await Task.Delay(500);
    }
}
