using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ESmokeTests.PageObjects;

public sealed class LoginPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public LoginPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public Task NavigateToAsync()
    {
        return _page.GotoAsync($"{_baseUrl}/login");
    }

    public Task EnterUsernameAsync(string username)
    {
        return _page.GetByPlaceholder("Username").FillAsync(username);
    }

    public Task EnterPasswordAsync(string password)
    {
        return _page.GetByPlaceholder("Password").FillAsync(password);
    }

    public Task ClickLoginAsync()
    {
        return _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
    }

    public async Task LoginAsAsync(string username, string password)
    {
        await NavigateToAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 15_000 });
        await EnterUsernameAsync(username);
        await EnterPasswordAsync(password);
        await ClickLoginAsync();
    }
}
