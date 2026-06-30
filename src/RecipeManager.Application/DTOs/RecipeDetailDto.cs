using RecipeManager.Domain.Enums;

namespace RecipeManager.Application.DTOs;

public record RecipeDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DifficultyLevel DifficultyLevel,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    string? ImageUrl,
    Guid CategoryId,
    string CategoryName,
    Guid UserId,
    IReadOnlyList<RecipeStepDto> Steps,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    DateTime DateCreated,
    DateTime DateUpdated);
