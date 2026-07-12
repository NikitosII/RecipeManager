using System.Net.Http.Headers;
using System.Net.Http.Json;
using RecipeManager.Application.DTOs;

namespace RecipeManager.Api.IntegrationTests;

internal static class ApiClientExtensions
{
    /// <summary>
    /// Registers a fresh (unique-email) user and returns the auth response.
    /// </summary>
    public static async Task<AuthResponseDto> RegisterUserAsync(this HttpClient client)
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = "Password1"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    }

    /// <summary>
    /// Registers a user and returns a client with the Bearer token pre-attached.
    /// </summary>
    public static async Task<(HttpClient Client, AuthResponseDto Auth)> AuthenticatedClientAsync(this ApiFactory factory)
    {
        var client = factory.CreateClient();
        var auth = await client.RegisterUserAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return (client, auth);
    }

    public static async Task<Guid> FirstCategoryIdAsync(this HttpClient client)
    {
        var categories = await client.GetFromJsonAsync<List<CategoryDto>>("/api/v1/categories");
        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
        return categories[0].Id;
    }
}

internal record CreatedIdResponse(Guid Id);
