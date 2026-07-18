using RecipeManager.Domain.Enums;

namespace RecipeManager.Domain.Nutrition;

/// <summary>
/// Computes a recipe's per-serving nutrition from its ingredients. Pure and
/// deterministic — it only ever reads the per-100g figures already cached on
/// each line, never any external source. An ingredient contributes only when it
/// has the four core macros AND its quantity can be converted to grams;
/// otherwise it is reported as uncounted so callers can be honest about coverage.
/// </summary>
public static class NutritionCalculator
{
    // US-customary volume equivalents in millilitres. These are deliberately the
    // rounded everyday values used on nutrition labels, not lab-exact figures.
    private const decimal MlPerTeaspoon = 5m;
    private const decimal MlPerTablespoon = 15m;
    private const decimal MlPerCup = 240m;
    private const decimal MlPerLiter = 1000m;
    private const decimal GramsPerKilogram = 1000m;

    public static RecipeNutrition Calculate(int servings, IReadOnlyCollection<NutritionLine> lines)
    {
        // Servings is validated as >= 1 upstream; guard anyway so we never divide by zero.
        if (servings <= 0) servings = 1;

        decimal calories = 0m, protein = 0m, fat = 0m, carbs = 0m, fiber = 0m;
        var anyFiber = false;
        var counted = 0;
        var uncounted = new List<UncountedIngredient>();

        foreach (var line in lines)
        {
            if (!HasCoreMacros(line))
            {
                uncounted.Add(new UncountedIngredient(line.IngredientName, UncountedReason.MissingNutritionData));
                continue;
            }

            var grams = ToGrams(line);
            if (grams is null)
            {
                uncounted.Add(new UncountedIngredient(line.IngredientName, UncountedReason.UnconvertibleUnit));
                continue;
            }

            var factor = grams.Value / 100m;
            calories += factor * line.CaloriesPer100g!.Value;
            protein += factor * line.ProteinPer100g!.Value;
            fat += factor * line.FatPer100g!.Value;
            carbs += factor * line.CarbsPer100g!.Value;
            if (line.FiberPer100g is { } fiberPer100g)
            {
                fiber += factor * fiberPer100g;
                anyFiber = true;
            }

            counted++;
        }

        var perServing = new NutritionFacts(
            Round(calories / servings),
            Round(protein / servings),
            Round(fat / servings),
            Round(carbs / servings),
            anyFiber ? Round(fiber / servings) : null);

        return new RecipeNutrition(perServing, counted, lines.Count, uncounted);
    }

    private static bool HasCoreMacros(NutritionLine line) =>
        line.CaloriesPer100g.HasValue &&
        line.ProteinPer100g.HasValue &&
        line.FatPer100g.HasValue &&
        line.CarbsPer100g.HasValue;

    /// <summary>
    /// Converts a line's quantity to grams, or null when it cannot be converted:
    /// volume units need a density hint, pieces need a per-piece weight, and a
    /// pinch is treated as negligible.
    /// </summary>
    private static decimal? ToGrams(NutritionLine line) => line.Unit switch
    {
        MeasurementUnit.Gram => line.Quantity,
        MeasurementUnit.Kilogram => line.Quantity * GramsPerKilogram,
        MeasurementUnit.Milliliter => FromVolume(line.Quantity, 1m, line.DensityGramsPerMl),
        MeasurementUnit.Liter => FromVolume(line.Quantity, MlPerLiter, line.DensityGramsPerMl),
        MeasurementUnit.Teaspoon => FromVolume(line.Quantity, MlPerTeaspoon, line.DensityGramsPerMl),
        MeasurementUnit.Tablespoon => FromVolume(line.Quantity, MlPerTablespoon, line.DensityGramsPerMl),
        MeasurementUnit.Cup => FromVolume(line.Quantity, MlPerCup, line.DensityGramsPerMl),
        MeasurementUnit.Piece => line.GramsPerPiece is { } gramsPerPiece ? line.Quantity * gramsPerPiece : null,
        _ => null // Pinch (and any future unit) is not convertible without more data.
    };

    private static decimal? FromVolume(decimal quantity, decimal mlPerUnit, decimal? densityGramsPerMl) =>
        densityGramsPerMl is { } density ? quantity * mlPerUnit * density : null;

    private static decimal Round(decimal value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);
}
