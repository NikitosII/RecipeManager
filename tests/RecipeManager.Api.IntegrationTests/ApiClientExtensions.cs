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

    /// <summary>
    /// Creates a category with a unique name/slug and returns its id.
    /// </summary>
    public static async Task<Guid> CreateCategoryAsync(this HttpClient client)
    {
        var slug = $"cat-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/api/v1/categories", new { name = slug, slug });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedIdResponse>();
        return created!.Id;
    }

    /// <summary>
    /// Creates a minimal recipe in the given category and returns its id.
    /// </summary>
    public static async Task<Guid> CreateRecipeAsync(this HttpClient client, Guid categoryId, string title = "Test Recipe")
    {
        var response = await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title,
            description = (string?)null,
            difficultyLevel = 1,
            prepTimeMinutes = 5,
            cookTimeMinutes = 5,
            servings = 2,
            categoryId
        });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedIdResponse>();
        return created!.Id;
    }
}

internal record CreatedIdResponse(Guid Id);
