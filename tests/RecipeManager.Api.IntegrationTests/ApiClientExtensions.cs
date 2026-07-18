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
    /// Creates a recipe in the given category and returns its id. Numeric/difficulty
    /// attributes default to a minimal recipe but can be overridden for filter tests.
    /// </summary>
    public static async Task<Guid> CreateRecipeAsync(
        this HttpClient client,
        Guid categoryId,
        string title = "Test Recipe",
        int difficultyLevel = 1,
        int prepTimeMinutes = 5,
        int cookTimeMinutes = 5,
        int servings = 2)
    {
        var response = await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title,
            description = (string?)null,
            difficultyLevel,
            prepTimeMinutes,
            cookTimeMinutes,
            servings,
            categoryId
        });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedIdResponse>();
        return created!.Id;
    }

    /// <summary>
    /// Adds an ingredient (by name) to a recipe and returns its global ingredient id.
    /// Ingredients are looked up/created by name, so passing the same name to two
    /// recipes links them to the same ingredient id.
    /// </summary>
    public static async Task<Guid> AddIngredientAsync(
        this HttpClient client,
        Guid recipeId,
        string name,
        decimal quantity = 1m,
        int unit = 2 /* Gram */)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/ingredients",
            new { name, quantity, unit });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<RecipeIngredientDto>();
        return created!.IngredientId;
    }
}

internal record RecipeIngredientDto(Guid IngredientId, string IngredientName, decimal Quantity, int Unit);

internal record CreatedIdResponse(Guid Id);
