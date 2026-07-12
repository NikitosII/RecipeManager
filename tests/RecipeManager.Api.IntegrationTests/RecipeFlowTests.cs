using System.Net;
using System.Net.Http.Json;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Enums;

namespace RecipeManager.Api.IntegrationTests;

[Collection("Api")]
public class RecipeFlowTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_Recipe_RequiresAuthentication()
    {
        var client = factory.CreateClient();
        var categoryId = await client.FirstCategoryIdAsync();

        var response = await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Anonymous",
            description = (string?)null,
            difficultyLevel = DifficultyLevel.Easy,
            prepTimeMinutes = 5,
            cookTimeMinutes = 5,
            servings = 1,
            categoryId
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FullRecipeLifecycle_CreateAddStepsAndIngredients_Persists()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.FirstCategoryIdAsync();

        // Create
        var createResponse = await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Classic Pancakes",
            description = "Fluffy weekend pancakes",
            difficultyLevel = DifficultyLevel.Easy,
            prepTimeMinutes = 10,
            cookTimeMinutes = 15,
            servings = 4,
            categoryId
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>();
        var recipeId = created!.Id;

        // Add steps (this is the path that regressed with the EF Guid bug)
        var step1 = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new { description = "Mix dry ingredients" });
        step1.EnsureSuccessStatusCode();
        var step2 = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new { description = "Whisk in milk and eggs" });
        step2.EnsureSuccessStatusCode();

        // Add ingredients
        var ing1 = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients",
            new { name = "Flour", quantity = 200m, unit = MeasurementUnit.Gram });
        ing1.EnsureSuccessStatusCode();
        var ing2 = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients",
            new { name = "Large Eggs", quantity = 2m, unit = MeasurementUnit.Piece });
        ing2.EnsureSuccessStatusCode();

        // Read back the full detail
        var detail = await client.GetFromJsonAsync<RecipeDetailDto>($"/api/v1/recipes/{recipeId}");
        Assert.NotNull(detail);
        Assert.Equal("Classic Pancakes", detail!.Title);
        Assert.Equal(new[] { 1, 2 }, detail.Steps.Select(s => s.StepNumber));
        Assert.Equal(2, detail.Ingredients.Count);
    }

    /// <summary>
    /// Regression guard for the EF Guid client-key bug: adding a child (step) to a
    /// persisted, tracked recipe used to emit an UPDATE against a not-yet-inserted
    /// row and throw DbUpdateConcurrencyException (HTTP 500). It must return 200.
    /// </summary>
    [Fact]
    public async Task AddingStepToPersistedRecipe_DoesNotThrowConcurrency()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.FirstCategoryIdAsync();

        var createResponse = await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Regression Recipe",
            description = (string?)null,
            difficultyLevel = DifficultyLevel.Medium,
            prepTimeMinutes = 5,
            cookTimeMinutes = 5,
            servings = 2,
            categoryId
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>();

        var stepResponse = await client.PostAsJsonAsync($"/api/v1/recipes/{created!.Id}/steps",
            new { description = "This step must persist without a concurrency error" });

        Assert.Equal(HttpStatusCode.OK, stepResponse.StatusCode);
        var step = await stepResponse.Content.ReadFromJsonAsync<RecipeStepDto>();
        Assert.Equal(1, step!.StepNumber);
    }

    [Fact]
    public async Task GetRecipes_ReturnsPaginatedResults()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.FirstCategoryIdAsync();
        await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Listed Recipe",
            description = (string?)null,
            difficultyLevel = DifficultyLevel.Easy,
            prepTimeMinutes = 1,
            cookTimeMinutes = 1,
            servings = 1,
            categoryId
        });

        // Anonymous listing is allowed.
        var anon = factory.CreateClient();
        var response = await anon.GetAsync("/api/v1/recipes?page=1&pageSize=5");

        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadFromJsonAsync<PagedResult>();
        Assert.NotNull(page);
        Assert.True(page!.TotalCount >= 1);
    }

    private record PagedResult(int Page, int PageSize, int TotalCount, int TotalPages);
}
