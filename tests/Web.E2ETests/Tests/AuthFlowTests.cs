using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ETests.Helpers;
using Web.E2ETests.PageObjects;

namespace Web.E2ETests.Tests;

[TestFixture]
[Category("E2E")]
public class AuthFlowTests : E2ETestBase
{
    [Test]
    public async Task CanRegisterViaUI()
    {
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        await Page.GotoAsync($"{ServerUrl}/signup");
        await Page.GetByPlaceholder("Username").FillAsync(username);
        await Page.GetByPlaceholder("Email (optional)").FillAsync("");
        await Page.GetByPlaceholder("Password").FillAsync(password);
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        await Page.GetByText("Feed").First.WaitForAsync();
        Assert.That(await Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Log in" }).IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task CanLoginViaUI()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        await ApiHelper.RegisterUserAsync(client, username, password);

        await Page.GotoAsync($"{ServerUrl}/login");
        await Page.GetByPlaceholder("Username").FillAsync(username);
        await Page.GetByPlaceholder("Password").FillAsync(password);
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Page.GetByText("Feed").First.WaitForAsync();
        Assert.That(await Page.GetByText("Feed").First.IsVisibleAsync(), Is.True);
        Assert.That(await Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Log in" }).IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task CanLogoutViaMenu()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        await ApiHelper.RegisterUserAsync(client, username, password);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();

        await Page.Locator(".dropdown-trigger").EvaluateAsync("el => el.click()");
        var logoutItem = Page.Locator("span.panel-block:has-text('Logout')");
        await logoutItem.WaitForAsync();
        await logoutItem.WaitForAsync();
        await logoutItem.EvaluateAsync("el => el.click()");

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Log in" }).WaitForAsync();
        Assert.That(await Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Sign up" }).IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task SessionPersistsAfterReload()
    {
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";

        var signupPage = new SignupPage(Page, ServerUrl);
        await signupPage.RegisterAsAsync(username, password);

        await Page.GetByText("Feed").First.WaitForAsync();
        await Page.ReloadAsync();
        await Page.GetByText("Feed").First.WaitForAsync();

        Assert.That(await Page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Log in" }).IsVisibleAsync(), Is.False);
    }
}
