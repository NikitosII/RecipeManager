namespace RecipeManager.Application.DTOs;

public record CollectionDetailDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime DateCreated,
    IReadOnlyList<RecipeSummaryDto> Recipes);
