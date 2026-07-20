using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Nutrition;

namespace RecipeManager.Application.DTOs;

/// <summary>
/// Per-serving nutrition for a recipe together with how much of it could actually be counted. 
/// </summary>
public record NutritionDto(
    NutritionMode Mode,
    decimal Calories,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrates,
    decimal? Fiber,
    bool IsComplete,
    bool HasAnyData,
    int CountedCount,
    int TotalCount,
    IReadOnlyList<UncountedIngredientDto> Uncounted);

/// <summary>An ingredient that could not be included in the automatic figures, and why.</summary>
public record UncountedIngredientDto(string Name, UncountedReason Reason);
