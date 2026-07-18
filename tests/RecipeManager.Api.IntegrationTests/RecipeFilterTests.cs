using System.Net.Http.Json;
using RecipeManager.Application.DTOs;

namespace RecipeManager.Api.IntegrationTests;

[Collection("Api")]
public class RecipeFilterTests(ApiFactory factory)
{
    private static async Task<IReadOnlyList<RecipeSummaryDto>> ListAsync(HttpClient client, string query)
    {
        var page = await client.GetFromJsonAsync<PaginatedResponse<RecipeSummaryDto>>($"/api/v1/recipes?{query}");
        return page!.Items;
    }

    private static async Task<IReadOnlyList<Guid>> ListIdsAsync(HttpClient client, string query)
        => (await ListAsync(client, query)).Select(r => r.Id).ToList();

    /// <summary>
    /// Exercises every filter dimension against a fresh, isolated category so the
    /// shared test database's other recipes can't leak into the assertions.
    /// </summary>
    [Fact]
    public async Task Recipes_CanBeFilteredByEveryDimension()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.CreateCategoryAsync();

        // Unique ingredient names so two recipes can deliberately share an ingredient id.
        var chicken = $"chicken-{Guid.NewGuid():N}";
        var rice = $"rice-{Guid.NewGuid():N}";
        var beef = $"beef-{Guid.NewGuid():N}";

        // Recipe A: Easy, prep 10, cook 20, serves 2 — chicken + rice.
        var aId = await client.CreateRecipeAsync(categoryId, "Quick Easy", difficultyLevel: 1, prepTimeMinutes: 10, cookTimeMinutes: 20, servings: 2);
        var chickenId = await client.AddIngredientAsync(aId, chicken);
        var riceId = await client.AddIngredientAsync(aId, rice);

        // Recipe B: Hard, prep 60, cook 90, serves 8 — chicken + beef.
        var bId = await client.CreateRecipeAsync(categoryId, "Slow Hard", difficultyLevel: 3, prepTimeMinutes: 60, cookTimeMinutes: 90, servings: 8);
        await client.AddIngredientAsync(bId, chicken); // same name -> same ingredient id as A
        var beefId = await client.AddIngredientAsync(bId, beef);

        // Baseline: both recipes are in the category.
        var all = await ListIdsAsync(client, $"categoryId={categoryId}");
        Assert.Contains(aId, all);
        Assert.Contains(bId, all);

        // Difficulty (Easy) -> only A.
        var easy = await ListIdsAsync(client, $"categoryId={categoryId}&difficulty=1");
        Assert.Equal(new[] { aId }, easy);

        // Max prep 30 min -> only A (A=10, B=60).
        var quickPrep = await ListIdsAsync(client, $"categoryId={categoryId}&maxPrepTime=30");
        Assert.Equal(new[] { aId }, quickPrep);

        // Max cook 30 min -> only A (A=20, B=90).
        var quickCook = await ListIdsAsync(client, $"categoryId={categoryId}&maxCookTime=30");
        Assert.Equal(new[] { aId }, quickCook);

        // Min servings 5 -> only B (A=2, B=8).
        var bigBatch = await ListIdsAsync(client, $"categoryId={categoryId}&minServings=5");
        Assert.Equal(new[] { bId }, bigBatch);

        // Single ingredient shared by both -> A and B.
        var withChicken = await ListIdsAsync(client, $"categoryId={categoryId}&ingredientIds={chickenId}");
        Assert.Equal(2, withChicken.Count);
        Assert.Contains(aId, withChicken);
        Assert.Contains(bId, withChicken);

        // Must contain ALL selected -> chicken + rice matches only A.
        var chickenAndRice = await ListIdsAsync(client, $"categoryId={categoryId}&ingredientIds={chickenId}&ingredientIds={riceId}");
        Assert.Equal(new[] { aId }, chickenAndRice);

        // chicken + beef matches only B.
        var chickenAndBeef = await ListIdsAsync(client, $"categoryId={categoryId}&ingredientIds={chickenId}&ingredientIds={beefId}");
        Assert.Equal(new[] { bId }, chickenAndBeef);

        // Combining dimensions: Easy AND max prep 30 -> only A; Easy AND min servings 5 -> none.
        var easyAndQuick = await ListIdsAsync(client, $"categoryId={categoryId}&difficulty=1&maxPrepTime=30");
        Assert.Equal(new[] { aId }, easyAndQuick);

        var easyAndBig = await ListIdsAsync(client, $"categoryId={categoryId}&difficulty=1&minServings=5");
        Assert.Empty(easyAndBig);
    }

    /// <summary>
    /// Sorting by name, creation date and average rating, in both directions,
    /// scoped to a fresh category so ordering is deterministic.
    /// </summary>
    [Fact]
    public async Task Recipes_CanBeSortedByNameDateAndRating()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.CreateCategoryAsync();

        // Created oldest -> newest in this order (small gaps keep DateCreated distinct).
        var zId = await client.CreateRecipeAsync(categoryId, "Zucchini Bake");
        await Task.Delay(15);
        var aId = await client.CreateRecipeAsync(categoryId, "Apple Pie");
        await Task.Delay(15);
        var mId = await client.CreateRecipeAsync(categoryId, "Mango Salad");

        // Ratings: Apple 5 (best), Mango 3, Zucchini 1 (worst).
        await client.PutAsJsonAsync($"/api/v1/recipes/{aId}/rating", new { value = 5 });
        await client.PutAsJsonAsync($"/api/v1/recipes/{mId}/rating", new { value = 3 });
        await client.PutAsJsonAsync($"/api/v1/recipes/{zId}/rating", new { value = 1 });

        // Name A->Z and Z->A.
        var byName = await ListAsync(client, $"categoryId={categoryId}&sortBy=1&sortDescending=false");
        Assert.Equal(new[] { aId, mId, zId }, byName.Select(r => r.Id));

        var byNameDesc = await ListAsync(client, $"categoryId={categoryId}&sortBy=1&sortDescending=true");
        Assert.Equal(new[] { zId, mId, aId }, byNameDesc.Select(r => r.Id));

        // Date, newest first (the default) and oldest first.
        var byNewest = await ListAsync(client, $"categoryId={categoryId}&sortBy=0&sortDescending=true");
        Assert.Equal(new[] { mId, aId, zId }, byNewest.Select(r => r.Id));

        var byOldest = await ListAsync(client, $"categoryId={categoryId}&sortBy=0&sortDescending=false");
        Assert.Equal(new[] { zId, aId, mId }, byOldest.Select(r => r.Id));

        // Rating, highest first and lowest first.
        var byRating = await ListAsync(client, $"categoryId={categoryId}&sortBy=2&sortDescending=true");
        Assert.Equal(new[] { aId, mId, zId }, byRating.Select(r => r.Id));

        var byRatingAsc = await ListAsync(client, $"categoryId={categoryId}&sortBy=2&sortDescending=false");
        Assert.Equal(new[] { zId, mId, aId }, byRatingAsc.Select(r => r.Id));
    }
}
