using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.Testing.Api;

public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> RegisterUserAsync(string username, string password, string? email = null)
    {
        var requestEmail = string.IsNullOrWhiteSpace(email)
            ? $"{username}@discman.local"
            : email;

        var response = await _http.PostAsJsonAsync("/api/users", new
        {
            Username = username,
            Password = password,
            Email = requestEmail
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Register returned null");
    }

    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("/api/users/authenticate", new
        {
            Username = username,
            Password = password
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Login returned null");
    }

    public HttpClient CreateAuthenticatedHttpClient(string token)
    {
        var client = new HttpClient { BaseAddress = _http.BaseAddress };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<CourseResponse> CreateCourseAsync(string token, string courseName, string layoutName, int numberOfHoles = 9)
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

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CourseResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Create course returned null");
    }

    public async Task<RoundResponse> StartRoundAsync(string token, Guid courseId, List<string> players)
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

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RoundResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Start round returned null");
    }

    public async Task UpdateScoreAsync(string token, Guid roundId, int holeIndex, int strokes, string username)
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

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteRoundAsync(string token, Guid roundId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/rounds/{roundId}/complete")
        {
            Content = JsonContent.Create(new
            {
                Base64Signature = "data:image/svg+xml;base64,dGVzdA=="
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<RoundResponse> GetRoundAsync(string token, Guid roundId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/rounds/{roundId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RoundResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Get round returned null");
    }

    public async Task<List<CourseResponse>> GetCoursesAsync(string token, string filter = "")
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/courses?filter={Uri.EscapeDataString(filter)}&latitude=0&longitude=0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CourseResponse>>(JsonOptions);
        return result ?? new List<CourseResponse>();
    }

    public async Task<HttpResponseMessage> GetFeedsRawAsync(string token, int pageNumber = 1, int pageSize = 20)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/feeds?pageNumber={pageNumber}&pageSize={pageSize}&itemType=");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> GetLeaderboardRawAsync(string token, bool onlyFriends = false, int month = 0)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/leaderboard?onlyFriends={onlyFriends}&month={month}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> GetYearSummaryRawAsync(string token, string username, int year)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{Uri.EscapeDataString(username)}/yearsummary/{year}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> ToggleLikeFeedItemAsync(string token, Guid feedItemId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/feeds/feedItems/{feedItemId}/like");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _http.SendAsync(request);
    }

    public async Task<HttpStatusCode> GetUnauthorizedAsync(string url)
    {
        var response = await _http.GetAsync(url);
        return response.StatusCode;
    }
}

public sealed class AuthResponse
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
}

public sealed class CourseResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class RoundResponse
{
    public Guid Id { get; set; }
    public bool IsCompleted { get; set; }
    public List<PlayerScoreResponse> PlayerScores { get; set; } = new();
}

public sealed class PlayerScoreResponse
{
    public string PlayerName { get; set; } = "";
    public List<HoleScoreResponse> Scores { get; set; } = new();
}

public sealed class HoleScoreResponse
{
    public int Strokes { get; set; }
    public HoleResponse Hole { get; set; } = new();
}

public sealed class HoleResponse
{
    public int Number { get; set; }
    public int Par { get; set; }
}
