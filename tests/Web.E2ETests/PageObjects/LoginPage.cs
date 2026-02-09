using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ETests.PageObjects;

public class LoginPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public LoginPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/login");
    }

    public async Task EnterUsernameAsync(string username)
    {
        await _page.GetByPlaceholder("Username").FillAsync(username);
    }

    public async Task EnterPasswordAsync(string password)
    {
        await _page.GetByPlaceholder("Password").FillAsync(password);
    }

    public async Task ClickLoginAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
    }

    public async Task LoginAsAsync(string username, string password)
    {
        await NavigateToAsync();
        await EnterUsernameAsync(username);
        await EnterPasswordAsync(password);
        await ClickLoginAsync();
    }
}
