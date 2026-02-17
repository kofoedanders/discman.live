using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ESmokeTests.PageObjects;
using Web.Testing.Api;
using Web.Testing.Seeding;

namespace Web.E2ESmokeTests.Tests;

[TestFixture]
[Category("E2E")]
public sealed class TournamentsTests : E2ETestBase
{
    [Test]
    [CancelAfter(120_000)]
    public async Task TournamentsPageShowsListAndNewButton()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);
        await TestUsers.EnsureSeedUserExistsAsync(api);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(TestUsers.SeededUsername, TestUsers.SeededPassword);

        var tournamentsPage = new TournamentsPage(Page, ServerUrl);
        await tournamentsPage.NavigateToAsync();
        await tournamentsPage.WaitForPageAsync();

        var newTournamentVisible = await tournamentsPage.IsNewTournamentButtonVisibleAsync();
        Assert.That(newTournamentVisible, Is.True, "Should show 'New Tournament' button");
    }
}
