using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.E2ESmokeTests.PageObjects;
using Web.Testing.Api;
using Web.Testing.Seeding;

namespace Web.E2ESmokeTests.Tests;

[TestFixture]
[Category("E2E")]
public sealed class CoursesTests : E2ETestBase
{
    [Test]
    [CancelAfter(120_000)]
    public async Task CoursesPageShowsListAndSearch()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);
        var login = await TestUsers.EnsureSeedUserExistsAsync(api);

        var courseName = $"E2E-{Guid.NewGuid():N}"[..12];
        await api.CreateCourseAsync(login.Token, courseName, "Layout A", 9);

        var loginPage = new LoginPage(Page, ServerUrl);
        await loginPage.LoginAsAsync(TestUsers.SeededUsername, TestUsers.SeededPassword);

        var coursesPage = new CoursesPage(Page, ServerUrl);
        await coursesPage.NavigateToAsync();
        await coursesPage.WaitForPageAsync();

        var newCourseVisible = await coursesPage.IsNewCourseButtonVisibleAsync();
        Assert.That(newCourseVisible, Is.True, "Should show '+ New Course' button");

        var searchVisible = await coursesPage.IsSearchInputVisibleAsync();
        Assert.That(searchVisible, Is.True, "Should show search input");

        var courseCount = await coursesPage.GetCourseCountAsync();
        Assert.That(courseCount, Is.GreaterThanOrEqualTo(1), "Should show at least one course");
    }
}
