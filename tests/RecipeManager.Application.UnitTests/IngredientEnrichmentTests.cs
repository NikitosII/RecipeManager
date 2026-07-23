using NSubstitute;
using RecipeManager.Application.Features.Ingredients;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Application.UnitTests;

public class IngredientEnrichmentTests
{
    private readonly INutritionProvider _nutrition = Substitute.For<INutritionProvider>();

    [Fact]
    public async Task EnrichAsync_ProviderHasData_AppliesMacrosAndConversionAndReturnsTrue()
    {
        var ingredient = new Ingredient("Olive oil");
        _nutrition.LookupAsync("Olive oil", Arg.Any<CancellationToken>())
            .Returns(new NutritionLookup(884m, 0m, 100m, 0m, 0m, DensityGramsPerMl: 0.92m, GramsPerPiece: null));

        var enriched = await IngredientEnrichment.EnrichAsync(ingredient, _nutrition, CancellationToken.None);

        Assert.True(enriched);
        Assert.True(ingredient.HasNutrition);
        Assert.Equal(884m, ingredient.CaloriesPer100g);
        Assert.Equal(0.92m, ingredient.DensityGramsPerMl);
    }

    [Fact]
    public async Task EnrichAsync_ProviderHasNoData_LeavesIngredientUntouchedAndReturnsFalse()
    {
        var ingredient = new Ingredient("Moon dust");
        _nutrition.LookupAsync("Moon dust", Arg.Any<CancellationToken>()).Returns((NutritionLookup?)null);

        var enriched = await IngredientEnrichment.EnrichAsync(ingredient, _nutrition, CancellationToken.None);

        Assert.False(enriched);
        Assert.False(ingredient.HasNutrition);
    }

    [Fact]
    public async Task EnrichAsync_TransientProviderFailure_Propagates()
    {
        var ingredient = new Ingredient("Flour");
        _nutrition.LookupAsync("Flour", Arg.Any<CancellationToken>())
            .Returns<NutritionLookup?>(_ => throw new NutritionProviderUnavailableException("429"));

        await Assert.ThrowsAsync<NutritionProviderUnavailableException>(
            () => IngredientEnrichment.EnrichAsync(ingredient, _nutrition, CancellationToken.None));
    }
}
