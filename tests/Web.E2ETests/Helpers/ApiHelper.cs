using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.E2ETests.Helpers;

public static class ApiHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static HttpClient CreateAuthenticatedClient(string token)
    {
        var baseAddress = new Uri(AppFixture.Instance.ServerUrl);
        var client = new HttpClient { BaseAddress = baseAddress };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<AuthResponse> RegisterUserAsync(
        HttpClient client,
        string username,
        string password,
        string? email = null)
    {
        var requestEmail = string.IsNullOrWhiteSpace(email)
            ? $"{username}@discman.local"
            : email;

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            Username = username,
            Password = password,
            Email = requestEmail
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Register returned null");
    }

    public static async Task<AuthResponse> LoginAsync(
        HttpClient client,
        string username,
        string password)
    {
        var response = await client.PostAsJsonAsync("/api/users/authenticate", new
        {
            Username = username,
            Password = password
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Login returned null");
    }

    public static async Task<CourseResponse> CreateCourseAsync(
        HttpClient client,
        string token,
        string courseName,
        string layoutName,
        int numberOfHoles = 9)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/courses")
        {
            Content = JsonContent.Create(new
            {
                CourseName = courseName,
                LayoutName = layoutName,
                NumberOfHoles = numberOfHoles,
                Latitude = 0,
                Longitude = 0
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CourseResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Create course returned null");
    }

    public static async Task<RoundResponse> StartRoundAsync(
        HttpClient client,
        string token,
        Guid courseId,
        List<string> players)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/rounds")
        {
            Content = JsonContent.Create(new
            {
                CourseId = courseId,
                Players = players,
                RoundName = "",
                ScoreMode = 0
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RoundResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Start round returned null");
    }

    public static async Task UpdateScoreAsync(
        HttpClient client,
        string token,
        Guid roundId,
        int holeIndex,
        int strokes,
        string username)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/rounds/{roundId}/scores")
        {
            Content = JsonContent.Create(new
            {
                HoleIndex = holeIndex,
                Strokes = strokes,
                StrokeOutcomes = Array.Empty<object>(),
                Username = username
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public static async Task CompleteRoundAsync(
        HttpClient client,
        string token,
        Guid roundId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/rounds/{roundId}/complete")
        {
            Content = JsonContent.Create(new
            {
                Base64Signature = "data:image/svg+xml;base64,dGVzdA=="
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
}

public class CourseResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public class RoundResponse
{
    public Guid Id { get; set; }
    public bool IsCompleted { get; set; }
    public List<PlayerScoreResponse> PlayerScores { get; set; } = new();
}

public class PlayerScoreResponse
{
    public string PlayerName { get; set; } = "";
    public List<HoleScoreResponse> Scores { get; set; } = new();
}

public class HoleScoreResponse
{
    public int Strokes { get; set; }
    public HoleResponse Hole { get; set; } = new();
}

public class HoleResponse
{
    public int Number { get; set; }
    public int Par { get; set; }
}
