namespace RecipeManager.Application.DTOs;

public record CollectionSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    int RecipeCount,
    DateTime DateCreated);
