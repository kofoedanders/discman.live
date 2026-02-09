using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ETests.Helpers;
using Web.E2ETests.PageObjects;

namespace Web.E2ETests.Tests;

[TestFixture]
[Category("E2E")]
public class CourseFlowTests : E2ETestBase
{
    [Test]
    public async Task CanBrowseCoursesPage()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var courseName = $"Course{Guid.NewGuid():N}"[..15];

        await ApiHelper.RegisterUserAsync(client, username, password);
        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
        var courseResponse = await ApiHelper.CreateCourseAsync(client, loginResponse.Token, courseName, "Layout A", 9);
        await ApiHelper.StartRoundAsync(client, loginResponse.Token, courseResponse.Id, new List<string> { username });

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();

        var coursesResponse = Page.WaitForResponseAsync("**/api/courses?filter=**");
        await Page.GotoAsync($"{ServerUrl}/courses");
        await coursesResponse;
        await Page.Locator(".panel .panel-block:visible").First.WaitForAsync();

        Assert.That(await Page.Locator($".panel-block:has-text('{courseName}'):visible").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task CanCreateCourseViaUI()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var courseName = $"Course{Guid.NewGuid():N}"[..15];

        await ApiHelper.RegisterUserAsync(client, username, password);
        await ApiHelper.LoginAsync(client, username, password);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();

        await Page.GotoAsync($"{ServerUrl}/courses");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "New Course" }).WaitForAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "New Course" }).ClickAsync();

        await Page.Locator(".modal.is-active input[type='text']").First.FillAsync(courseName);
        await Page.Locator(".modal.is-active").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Page.Locator(".modal.is-active").WaitForAsync(new Microsoft.Playwright.LocatorWaitForOptions
        {
            State = Microsoft.Playwright.WaitForSelectorState.Detached
        });

        await Page.Locator($".panel-block:has-text('{courseName}')").WaitForAsync();
        Assert.That(await Page.Locator($".panel-block:has-text('{courseName}')").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task CanViewCourseDetails()
    {
        var client = AppFixture.Instance.HttpClient;
        var username = $"e2e{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        var courseName = $"Course{Guid.NewGuid():N}"[..15];

        await ApiHelper.RegisterUserAsync(client, username, password);
        var loginResponse = await ApiHelper.LoginAsync(client, username, password);
        var courseResponse = await ApiHelper.CreateCourseAsync(client, loginResponse.Token, courseName, "Layout A", 9);
        await ApiHelper.StartRoundAsync(client, loginResponse.Token, courseResponse.Id, new List<string> { username });

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(username, password);

        var homePage = new HomePage(Page, ServerUrl);
        await homePage.WaitForFeedAsync();

        var courseDetailsResponse = Page.WaitForResponseAsync("**/api/courses?filter=**");
        await Page.GotoAsync($"{ServerUrl}/courses/{courseName}");
        await courseDetailsResponse;
        await Page.Locator("h1.title").WaitForAsync();

        var titleText = await Page.Locator("h1.title").InnerTextAsync();
        Assert.That(titleText, Does.Contain(courseName));
        Assert.That(await Page.Locator("th:text-is('Hole')").First.IsVisibleAsync(), Is.True);
        Assert.That(await Page.Locator("th:text-is('Par')").First.IsVisibleAsync(), Is.True);
    }
}
