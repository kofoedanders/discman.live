using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ESmokeTests.PageObjects;

public sealed class HomePage
{
    private readonly IPage _page;

    public HomePage(IPage page)
    {
        _page = page;
    }

    public Task WaitForFeedAsync()
    {
        return _page.GetByText("Feed").First.WaitForAsync(new() { Timeout = 15_000 });
    }
}
