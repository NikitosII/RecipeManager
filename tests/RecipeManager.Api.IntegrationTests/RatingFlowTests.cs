using System.Net;
using System.Net.Http.Json;
using RecipeManager.Application.DTOs;

namespace RecipeManager.Api.IntegrationTests;

[Collection("Api")]
public class RatingFlowTests(ApiFactory factory)
{
    private static async Task<RecipeDetailDto> GetDetailAsync(HttpClient client, Guid recipeId)
        => (await client.GetFromJsonAsync<RecipeDetailDto>($"/api/v1/recipes/{recipeId}"))!;

    [Fact]
    public async Task Rating_IsAveragedAcrossUsersAndEditable()
    {
        var (owner, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await owner.CreateCategoryAsync();
        var recipeId = await owner.CreateRecipeAsync(categoryId, "Rate me");

        // First user rates 4.
        var rate1 = await owner.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/rating", new { value = 4 });
        Assert.Equal(HttpStatusCode.NoContent, rate1.StatusCode);

        var afterFirst = await GetDetailAsync(owner, recipeId);
        Assert.Equal(4.0, afterFirst.AverageRating);
        Assert.Equal(1, afterFirst.RatingCount);
        Assert.Equal(4, afterFirst.UserRating);

        // A second user rates 2 → average is (4 + 2) / 2 = 3.
        var (stranger, _) = await factory.AuthenticatedClientAsync();
        await stranger.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/rating", new { value = 2 });

        var afterSecond = await GetDetailAsync(owner, recipeId);
        Assert.Equal(3.0, afterSecond.AverageRating);
        Assert.Equal(2, afterSecond.RatingCount);
        Assert.Equal(4, afterSecond.UserRating); // still the owner's own value

        // The owner changes their rating to 5 → average is (5 + 2) / 2 = 3.5, count unchanged.
        await owner.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/rating", new { value = 5 });
        var afterUpdate = await GetDetailAsync(owner, recipeId);
        Assert.Equal(3.5, afterUpdate.AverageRating);
        Assert.Equal(2, afterUpdate.RatingCount);
        Assert.Equal(5, afterUpdate.UserRating);

        // The owner removes their rating → only the stranger's 2 remains.
        var removed = await owner.DeleteAsync($"/api/v1/recipes/{recipeId}/rating");
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);

        var afterRemoval = await GetDetailAsync(owner, recipeId);
        Assert.Equal(2.0, afterRemoval.AverageRating);
        Assert.Equal(1, afterRemoval.RatingCount);
        Assert.Null(afterRemoval.UserRating);
    }

    [Fact]
    public async Task Rating_OutOfRange_IsRejected()
    {
        var (client, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await client.CreateCategoryAsync();
        var recipeId = await client.CreateRecipeAsync(categoryId);

        var response = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/rating", new { value = 9 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rating_RequiresAuthentication()
    {
        var (owner, _) = await factory.AuthenticatedClientAsync();
        var categoryId = await owner.CreateCategoryAsync();
        var recipeId = await owner.CreateRecipeAsync(categoryId);

        var anon = factory.CreateClient();
        var response = await anon.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/rating", new { value = 3 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
