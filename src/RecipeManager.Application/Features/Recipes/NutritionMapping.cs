using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Nutrition;

namespace RecipeManager.Application.Features.Recipes;

/// <summary>
/// Resolves the per-serving nutrition the API exposes for a recipe.
/// </summary>
internal static class NutritionMapping
{
    public static NutritionDto Resolve(Recipe recipe) =>
        recipe.NutritionMode == NutritionMode.Manual
            ? Manual(recipe)
            : Auto(recipe);

    private static NutritionDto Manual(Recipe recipe)
    {
        // The author supplied the figures, so coverage is complete by definition.
        var count = recipe.RecipeIngredients.Count;
        return new NutritionDto(
            NutritionMode.Manual,
            recipe.ManualCalories ?? 0m,
            recipe.ManualProtein ?? 0m,
            recipe.ManualFat ?? 0m,
            recipe.ManualCarbohydrates ?? 0m,
            recipe.ManualFiber,
            IsComplete: true,
            HasAnyData: true,
            CountedCount: count,
            TotalCount: count,
            Uncounted: []);
    }

    private static NutritionDto Auto(Recipe recipe)
    {
        var lines = recipe.RecipeIngredients.Select(ToLine).ToList();
        var nutrition = NutritionCalculator.Calculate(recipe.Servings, lines);

        return new NutritionDto(
            NutritionMode.Auto,
            nutrition.PerServing.Calories,
            nutrition.PerServing.Protein,
            nutrition.PerServing.Fat,
            nutrition.PerServing.Carbohydrates,
            nutrition.PerServing.Fiber,
            nutrition.IsComplete,
            nutrition.HasAnyData,
            nutrition.CountedCount,
            nutrition.TotalCount,
            nutrition.Uncounted.Select(u => new UncountedIngredientDto(u.Name, u.Reason)).ToList());
    }

    private static NutritionLine ToLine(RecipeIngredient ri)
    {
        var ingredient = ri.Ingredient;
        return new NutritionLine(
            ingredient?.Name ?? string.Empty,
            ri.Quantity,
            ri.Unit,
            ingredient?.CaloriesPer100g,
            ingredient?.ProteinPer100g,
            ingredient?.FatPer100g,
            ingredient?.CarbsPer100g,
            ingredient?.FiberPer100g,
            ingredient?.DensityGramsPerMl,
            ingredient?.GramsPerPiece);
    }
}
