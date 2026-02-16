using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ESmokeTests.PageObjects;

public sealed class RoundPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public RoundPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public Task NavigateToAsync(Guid roundId)
    {
        return _page.GotoAsync($"{_baseUrl}/rounds/{roundId}");
    }

    public Task WaitForRoundContentAsync()
    {
        return _page.GetByText("Par").First.WaitForAsync(new() { Timeout = 15_000 });
    }
}
