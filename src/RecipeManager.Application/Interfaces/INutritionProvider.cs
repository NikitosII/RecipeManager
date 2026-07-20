namespace RecipeManager.Application.Interfaces;

/// <summary>
/// Per-100g macros looked up from the nutrition source, plus optional unit-conversion
/// hints derived from the source's portion data. 
/// </summary>
public record NutritionLookup(
    decimal CaloriesPer100g,
    decimal ProteinPer100g,
    decimal FatPer100g,
    decimal CarbsPer100g,
    decimal? FiberPer100g,
    decimal? DensityGramsPerMl = null,
    decimal? GramsPerPiece = null);

/// <summary>
/// Looks up an ingredient's nutrition from an external source. 
/// </summary>
public interface INutritionProvider
{
    Task<NutritionLookup?> LookupAsync(string ingredientName, CancellationToken cancellationToken = default);
}
