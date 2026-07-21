using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Application.Features.Ingredients;

/// <summary>
/// Shared nutrition enrichment for ingredients created on demand — used both by the 
/// standalone create endpoint and by the recipe editor.
/// </summary>
internal static class IngredientEnrichment
{
    public static async Task EnrichAsync(
        Ingredient ingredient, INutritionProvider nutritionProvider, CancellationToken cancellationToken)
    {
        var facts = await nutritionProvider.LookupAsync(ingredient.Name, cancellationToken);
        if (facts is not null)
            ingredient.SetNutritionFacts(
                facts.CaloriesPer100g, facts.ProteinPer100g, facts.FatPer100g,
                facts.CarbsPer100g, facts.FiberPer100g);
    }
}
