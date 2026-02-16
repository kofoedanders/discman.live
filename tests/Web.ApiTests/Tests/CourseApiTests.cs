using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing.Api;

namespace Web.ApiTests.Tests;

[TestFixture]
public sealed class CourseApiTests
{
    [Test]
    public async Task CanCreateAndListCourses()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var courseName = $"Course{Guid.NewGuid():N}"[..15];
        var created = await api.CreateCourseAsync(login.Token, courseName, "Layout A", 9);

        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.Name, Is.Not.Null.And.Not.Empty);

        var courses = await api.GetCoursesAsync(login.Token, courseName);
        Assert.That(courses, Has.Count.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task CanCreateCourseWithRound()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"api{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var courseName = $"Course{Guid.NewGuid():N}"[..15];
        var course = await api.CreateCourseAsync(login.Token, courseName, "Layout B", 9);
        var round = await api.StartRoundAsync(login.Token, course.Id, new List<string> { username });

        Assert.That(round.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(round.IsCompleted, Is.False);
    }
}
