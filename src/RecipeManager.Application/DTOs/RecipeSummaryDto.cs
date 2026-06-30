using RecipeManager.Domain.Enums;

namespace RecipeManager.Application.DTOs;

public record RecipeSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    DifficultyLevel DifficultyLevel,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    string? ImageUrl,
    string CategoryName,
    DateTime DateCreated);
