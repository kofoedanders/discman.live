using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ETests.PageObjects;

public class RoundPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public RoundPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateToAsync(Guid roundId)
    {
        await _page.GotoAsync($"{_baseUrl}/rounds/{roundId}");
    }

    public async Task WaitForRoundLoadedAsync()
    {
        await _page.Locator(".is-size-5").First.WaitForAsync();
    }

    public async Task<bool> IsRoundCompletedAsync()
    {
        try
        {
            await _page.Locator(".tabs a:text-is('Scores')")
                .WaitForAsync(new() { Timeout = 10_000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task<string> GetCourseName()
    {
        return await _page.Locator(".is-size-5").First.InnerTextAsync();
    }

    public async Task WaitForScoreCardAsync()
    {
        await _page.Locator("th:text-is('Hole')").WaitForAsync();
    }

    public async Task WaitForSignRoundAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign Round" }).WaitForAsync();
    }

    public async Task ClickSignRoundAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign Round" }).ClickAsync();
    }
}
