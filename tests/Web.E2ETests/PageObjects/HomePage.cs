using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ETests.PageObjects;

public class HomePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public HomePage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/");
    }

    public async Task WaitForFeedAsync()
    {
        await _page.GetByText("Feed").First.WaitForAsync();
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var feedVisible = await _page.GetByText("Feed").First.IsVisibleAsync();
        var loginVisible = await _page.GetByRole(AriaRole.Link, new() { Name = "Log in" }).IsVisibleAsync();
        var signupVisible = await _page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).IsVisibleAsync();
        return feedVisible && !loginVisible && !signupVisible;
    }
}
