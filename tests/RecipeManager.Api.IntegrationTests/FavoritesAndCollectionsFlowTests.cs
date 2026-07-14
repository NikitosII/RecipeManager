using System.Net;
using System.Net.Http.Json;
using RecipeManager.Application.DTOs;

namespace RecipeManager.Api.IntegrationTests;

[Collection("Api")]
public class FavoritesAndCollectionsFlowTests(ApiFactory factory)
{
    [Fact]
    public async Task Favorites_AddListAndRemove_RoundTrips()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.CreateCategoryAsync();
        var recipeId = await client.CreateRecipeAsync(categoryId, "Favourite me");

        // Add
        var add = await client.PutAsync($"/api/v1/favorites/{recipeId}", content: null);
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        // The recipe now reports IsFavorite for this user, both in its detail and in the favourites list.
        var detail = await client.GetFromJsonAsync<RecipeDetailDto>($"/api/v1/recipes/{recipeId}");
        Assert.True(detail!.IsFavorite);

        var favorites = await client.GetFromJsonAsync<PaginatedResponse<RecipeSummaryDto>>("/api/v1/favorites");
        Assert.Contains(favorites!.Items, r => r.Id == recipeId && r.IsFavorite);

        // Remove
        var remove = await client.DeleteAsync($"/api/v1/favorites/{recipeId}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);

        var afterRemoval = await client.GetFromJsonAsync<PaginatedResponse<RecipeSummaryDto>>("/api/v1/favorites");
        Assert.DoesNotContain(afterRemoval!.Items, r => r.Id == recipeId);
    }

    [Fact]
    public async Task Favorites_RequireAuthentication()
    {
        var anon = factory.CreateClient();
        var response = await anon.GetAsync("/api/v1/favorites");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Collections_CreateAddRecipeAndReadBack()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.CreateCategoryAsync();
        var recipeId = await client.CreateRecipeAsync(categoryId, "In a collection");

        // Create a collection
        var createResponse = await client.PostAsJsonAsync("/api/v1/collections",
            new { name = "Weeknight dinners", description = "Fast and easy" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var collectionId = (await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>())!.Id;

        // Add the recipe to it
        var add = await client.PutAsync($"/api/v1/collections/{collectionId}/recipes/{recipeId}", content: null);
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        // The detail view lists the recipe
        var detail = await client.GetFromJsonAsync<CollectionDetailDto>($"/api/v1/collections/{collectionId}");
        Assert.Equal("Weeknight dinners", detail!.Name);
        Assert.Contains(detail.Recipes, r => r.Id == recipeId);

        // The summary list reports the count
        var collections = await client.GetFromJsonAsync<List<CollectionSummaryDto>>("/api/v1/collections");
        Assert.Contains(collections!, c => c.Id == collectionId && c.RecipeCount == 1);

        // Remove the recipe, then delete the collection
        var removeRecipe = await client.DeleteAsync($"/api/v1/collections/{collectionId}/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.NoContent, removeRecipe.StatusCode);

        var deleteCollection = await client.DeleteAsync($"/api/v1/collections/{collectionId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteCollection.StatusCode);
    }

    [Fact]
    public async Task Collections_ArePrivateToTheirOwner()
    {
        var (owner, _) = await factory.AuthenticatedClientAsync();
        var createResponse = await owner.PostAsJsonAsync("/api/v1/collections",
            new { name = "Private", description = (string?)null });
        var collectionId = (await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>())!.Id;

        var (stranger, _) = await factory.AuthenticatedClientAsync();
        var forbidden = await stranger.GetAsync($"/api/v1/collections/{collectionId}");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }
}
