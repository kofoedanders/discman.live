using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Web.E2ETests.Helpers;

public static class ApiHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<AuthResponse> RegisterUserAsync(
        HttpClient client,
        string username,
        string password,
        string email = "")
    {
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            Username = username,
            Password = password,
            Email = email
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
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
}
