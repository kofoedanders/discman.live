using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ESmokeTests.PageObjects;

public sealed class ProfilePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ProfilePage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public Task NavigateToAsync()
    {
        return _page.GotoAsync($"{_baseUrl}/profile");
    }

    public Task WaitForPageAsync()
    {
        return _page.GetByTestId("profile-page").WaitForAsync(new() { Timeout = 15_000 });
    }

    public Task<bool> IsProfileCardVisibleAsync()
    {
        return _page.GetByText("Elo:").IsVisibleAsync();
    }

    public Task<bool> IsRoundsTabVisibleAsync()
    {
        return _page.GetByRole(AriaRole.Button, new() { Name = "Rounds" }).IsVisibleAsync();
    }

    public Task<bool> IsAchievementsTabVisibleAsync()
    {
        return _page.GetByRole(AriaRole.Button, new() { Name = "Achievements" }).IsVisibleAsync();
    }

    public Task ClickRoundsTabAsync()
    {
        return _page.GetByRole(AriaRole.Button, new() { Name = "Rounds" }).ClickAsync();
    }

    public Task ClickAchievementsTabAsync()
    {
        return _page.GetByRole(AriaRole.Button, new() { Name = "Achievements" }).ClickAsync();
    }
}
