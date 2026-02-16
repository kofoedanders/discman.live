using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Web.Testing.Api;

namespace Web.IntegrationTests.Tests;

[TestFixture]
public sealed class CourseCrudTests
{
    [Test]
    public async Task CanCreateMultipleCoursesAndFilter()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);

        var username = $"int{Guid.NewGuid():N}"[..12];
        var password = "TestPass123!";
        await api.RegisterUserAsync(username, password);
        var login = await api.LoginAsync(username, password);

        var prefix = $"IntCrs{Guid.NewGuid():N}"[..8];
        var course1Name = $"{prefix}-Alpha";
        var course2Name = $"{prefix}-Beta";

        await api.CreateCourseAsync(login.Token, course1Name, "Layout A", 9);
        await api.CreateCourseAsync(login.Token, course2Name, "Layout B", 18);

        var courses = await api.GetCoursesAsync(login.Token, prefix);
        Assert.That(courses, Has.Count.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task CoursesListRequiresAuth()
    {
        var client = AppFixture.Instance.HttpClient;
        var api = new ApiClient(client);
        var status = await api.GetUnauthorizedAsync("/api/courses?filter=&latitude=0&longitude=0");
        Assert.That(status, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
