using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Web.E2ETests.PageObjects;

public class SignupPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public SignupPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/signup");
    }

    public async Task EnterUsernameAsync(string username)
    {
        await _page.GetByPlaceholder("Username").FillAsync(username);
    }

    public async Task EnterEmailAsync(string email)
    {
        await _page.GetByPlaceholder("Email (optional)").FillAsync(email);
    }

    public async Task EnterPasswordAsync(string password)
    {
        await _page.GetByPlaceholder("Password").FillAsync(password);
    }

    public async Task ClickRegisterAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
    }

    public async Task RegisterAsAsync(string username, string password, string email = "")
    {
        await NavigateToAsync();
        await EnterUsernameAsync(username);
        if (!string.IsNullOrEmpty(email))
        {
            await EnterEmailAsync(email);
        }
        await EnterPasswordAsync(password);
        await ClickRegisterAsync();
    }
}
