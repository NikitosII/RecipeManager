using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes;

/// <summary>
/// Shared projection from <see cref="Recipe"/> to <see cref="RecipeSummaryDto"/>,
/// reused by the recipe listing, favourites and collection-detail queries.
/// </summary>
internal static class RecipeMapping
{
    public static RecipeSummaryDto ToSummary(
        Recipe recipe, string authorName, bool isFavorite, RecipeRatingSummary? rating) => new(
        recipe.Id,
        recipe.Title,
        recipe.Description,
        recipe.DifficultyLevel,
        recipe.PrepTimeMinutes,
        recipe.CookTimeMinutes,
        recipe.Servings,
        recipe.ImageUrl,
        recipe.Category?.Name ?? string.Empty,
        authorName,
        isFavorite,
        rating?.Average ?? 0,
        rating?.Count ?? 0,
        rating?.UserValue,
        recipe.DateCreated);
}
