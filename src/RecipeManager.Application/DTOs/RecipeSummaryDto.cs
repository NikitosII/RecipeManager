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
    string AuthorName,
    bool IsFavorite,
    double AverageRating,
    int RatingCount,
    int? UserRating,
    DateTime DateCreated);
