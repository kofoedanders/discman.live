using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ESmokeTests.PageObjects;

public sealed class TournamentsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public TournamentsPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public Task NavigateToAsync()
    {
        return _page.GotoAsync($"{_baseUrl}/tournaments");
    }

    public Task WaitForPageAsync()
    {
        return _page.GetByTestId("tournaments-page").WaitForAsync(new() { Timeout = 15_000 });
    }

    public Task<bool> IsNewTournamentButtonVisibleAsync()
    {
        return _page.GetByRole(AriaRole.Link, new() { Name = "New Tournament" }).IsVisibleAsync();
    }

    public async Task<int> GetTournamentCountAsync()
    {
        var container = _page.GetByTestId("tournaments-page");
        var links = container.Locator("a[href^='/tournaments/']");
        return await links.CountAsync();
    }
}
