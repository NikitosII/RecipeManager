using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Application.Features.Ingredients;

/// <summary>
/// Shared nutrition enrichment for an ingredient — run by the background enrichment
/// worker that drains the <see cref="IIngredientEnrichmentQueue"/>.
/// </summary>
public static class IngredientEnrichment
{
    /// <summary>
    /// Looks the ingredient's macros up from the provider and applies them in place.
    /// Returns <c>true</c> when data was applied, <c>false</c> when the provider had no usable data. 
    /// </summary>
    public static async Task<bool> EnrichAsync(
        Ingredient ingredient, INutritionProvider nutritionProvider, CancellationToken cancellationToken)
    {
        var facts = await nutritionProvider.LookupAsync(ingredient.Name, cancellationToken);
        if (facts is null)
            return false;

        ingredient.SetNutritionFacts(
            facts.CaloriesPer100g, facts.ProteinPer100g, facts.FatPer100g,
            facts.CarbsPer100g, facts.FiberPer100g);

        // Cache conversion hints so volume/piece quantities can be turned into grams.
        if (facts.DensityGramsPerMl is not null || facts.GramsPerPiece is not null)
            ingredient.SetConversion(facts.DensityGramsPerMl, facts.GramsPerPiece);

        return true;
    }
}
