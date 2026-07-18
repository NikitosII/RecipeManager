using RecipeManager.Domain.Common;

namespace RecipeManager.Domain.Entities;

public class Ingredient : BaseEntity
{
    protected Ingredient() { }

    public Ingredient(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = default!;

    // -- Cached nutrition (per 100 g) -- //
    public decimal? CaloriesPer100g { get; private set; }
    public decimal? ProteinPer100g { get; private set; }
    public decimal? FatPer100g { get; private set; }
    public decimal? CarbsPer100g { get; private set; }
    public decimal? FiberPer100g { get; private set; }

    // -- Unit-conversion hints, used to turn volume/piece quantities into grams -- //
    public decimal? DensityGramsPerMl { get; private set; }
    public decimal? GramsPerPiece { get; private set; }

    /// <summary>True once the core macros needed for a calculation are cached.</summary>
    public bool HasNutrition =>
        CaloriesPer100g.HasValue && ProteinPer100g.HasValue &&
        FatPer100g.HasValue && CarbsPer100g.HasValue;

    public void Update(string name)
    {
        Name = name;
        DateUpdated = DateTime.UtcNow;
    }

    /// <summary>Caches the per-100g macros looked up from the nutrition source.</summary>
    public void SetNutritionFacts(
        decimal? caloriesPer100g,
        decimal? proteinPer100g,
        decimal? fatPer100g,
        decimal? carbsPer100g,
        decimal? fiberPer100g)
    {
        CaloriesPer100g = caloriesPer100g;
        ProteinPer100g = proteinPer100g;
        FatPer100g = fatPer100g;
        CarbsPer100g = carbsPer100g;
        FiberPer100g = fiberPer100g;
        DateUpdated = DateTime.UtcNow;
    }

    /// <summary>Caches the hints used to convert volume/piece quantities to grams.</summary>
    public void SetConversion(decimal? densityGramsPerMl, decimal? gramsPerPiece)
    {
        DensityGramsPerMl = densityGramsPerMl;
        GramsPerPiece = gramsPerPiece;
        DateUpdated = DateTime.UtcNow;
    }
}
