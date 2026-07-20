namespace RecipeManager.Application.DTOs;

/// <summary>
/// An ingredient's cached per-100g nutrition, returned after a re-fetch so callers
/// can see whether the lookup found usable data.
/// </summary>
public record IngredientNutritionDto(
    Guid Id,
    string Name,
    bool HasNutrition,
    decimal? CaloriesPer100g,
    decimal? ProteinPer100g,
    decimal? FatPer100g,
    decimal? CarbsPer100g,
    decimal? FiberPer100g);
