using RecipeManager.Domain.Enums;

namespace RecipeManager.Domain.Nutrition;

/// <summary>
/// One recipe ingredient reduced to just what the calculator needs: how much is
/// used (quantity + unit) and the ingredient's cached per-100g nutrition and
/// unit-conversion hints. Kept free of entity types so it is trivial to build
/// in tests and to map from persistence in the Application layer.
/// </summary>
public sealed record NutritionLine(
    string IngredientName,
    decimal Quantity,
    MeasurementUnit Unit,
    decimal? CaloriesPer100g,
    decimal? ProteinPer100g,
    decimal? FatPer100g,
    decimal? CarbsPer100g,
    decimal? FiberPer100g,
    decimal? DensityGramsPerMl,
    decimal? GramsPerPiece);
