using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Web.Testing.Api;

namespace Web.Testing.Seeding;

public static class TestUsers
{
    public const string SeededUsername = "seeded-e2e";
    public const string SeededPassword = "TestPass123!";

    public static async Task<AuthResponse> EnsureSeedUserExistsAsync(ApiClient api)
    {
        try
        {
            return await api.RegisterUserAsync(SeededUsername, SeededPassword);
        }
        catch (HttpRequestException)
        {
            // User already exists — just login instead
            return await api.LoginAsync(SeededUsername, SeededPassword);
        }
    }
}
