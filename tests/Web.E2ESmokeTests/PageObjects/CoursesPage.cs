using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ESmokeTests.PageObjects;

public sealed class CoursesPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public CoursesPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public Task NavigateToAsync()
    {
        return _page.GotoAsync($"{_baseUrl}/courses");
    }

    public Task WaitForPageAsync()
    {
        return _page.GetByTestId("courses-page").WaitForAsync(new() { Timeout = 15_000 });
    }

    public Task<bool> IsNewCourseButtonVisibleAsync()
    {
        return _page.GetByRole(AriaRole.Link, new() { Name = "+ New Course" }).IsVisibleAsync();
    }

    public Task<bool> IsSearchInputVisibleAsync()
    {
        return _page.GetByPlaceholder("Search courses...").IsVisibleAsync();
    }

    public Task SearchAsync(string text)
    {
        return _page.GetByPlaceholder("Search courses...").FillAsync(text);
    }

    public Task ClickNewCourseAsync()
    {
        return _page.GetByRole(AriaRole.Link, new() { Name = "+ New Course" }).ClickAsync();
    }

    public async Task<int> GetCourseCountAsync()
    {
        var container = _page.GetByTestId("courses-page");
        var links = container.Locator("a[href^='/courses/']");
        return await links.CountAsync();
    }
}
