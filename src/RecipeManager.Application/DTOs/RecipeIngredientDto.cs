using RecipeManager.Domain.Enums;

namespace RecipeManager.Application.DTOs;

public record RecipeIngredientDto(
    Guid IngredientId,
    string IngredientName,
    decimal Quantity,
    MeasurementUnit Unit);
