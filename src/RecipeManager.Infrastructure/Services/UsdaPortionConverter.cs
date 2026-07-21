namespace RecipeManager.Infrastructure.Services;

/// <summary>A single USDA food portion, flattened to just the fields we derive from.</summary>
public sealed record UsdaPortion(decimal? Amount, decimal? GramWeight, string? Modifier, string? MeasureUnitName);

/// <summary>
/// Derives unit-conversion hints from a food's USDA portions
/// </summary>
public static class UsdaPortionConverter
{
    // Everyday volume equivalents in millilitres — the same rounded values the calculator uses.
    private const decimal MlPerTeaspoon = 5m;
    private const decimal MlPerTablespoon = 15m;
    private const decimal MlPerCup = 240m;
    private const decimal MlPerMilliliter = 1m;
    private const decimal MlPerLiter = 1000m;
    private const decimal MlPerFluidOunce = 29.5735m;

    public static (decimal? Density, decimal? GramsPerPiece) Derive(IReadOnlyCollection<UsdaPortion>? portions)
    {
        if (portions is null || portions.Count == 0)
            return (null, null);

        var densities = new List<decimal>();
        var pieces = new List<(string Unit, decimal Grams)>();

        foreach (var portion in portions)
        {
            if (portion.GramWeight is not { } grams || grams <= 0m ||
                portion.Amount is not { } amount || amount <= 0m)
                continue;

            var unit = UnitText(portion);
            if (unit.Length == 0)
                continue;

            if (VolumeMl(unit) is { } ml)
                densities.Add(grams / (amount * ml));
            else if (IsPiece(unit))
                pieces.Add((unit, grams / amount));
        }

        var density = densities.Count > 0
            ? Math.Round(densities.Average(), 4, MidpointRounding.AwayFromZero)
            : (decimal?)null;

        decimal? gramsPerPiece = null;
        if (pieces.Count > 0)
        {
            // Prefer a "medium" portion as the typical piece; otherwise take the first.
            var chosen = pieces.FirstOrDefault(p => p.Unit.Contains("medium"));
            var grams = chosen.Grams > 0m ? chosen.Grams : pieces[0].Grams;
            gramsPerPiece = Math.Round(grams, 2, MidpointRounding.AwayFromZero);
        }

        return (density, gramsPerPiece);
    }

    // The measure unit name if it carries meaning, else the free-text modifier.
    private static string UnitText(UsdaPortion portion)
    {
        var name = portion.MeasureUnitName?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(name) && name != "undetermined")
            return name;
        return portion.Modifier?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static decimal? VolumeMl(string unit)
    {
        if (unit.Contains("tablespoon") || unit.Contains("tbsp")) return MlPerTablespoon;
        if (unit.Contains("teaspoon") || unit.Contains("tsp")) return MlPerTeaspoon;
        if (unit.Contains("cup")) return MlPerCup;
        if (unit.Contains("fluid ounce") || unit.Contains("fl oz")) return MlPerFluidOunce;
        if (unit.Contains("milliliter") || unit.Contains("millilitre") || unit == "ml") return MlPerMilliliter;
        if (unit.Contains("liter") || unit.Contains("litre") || unit == "l") return MlPerLiter;
        return null;
    }

    private static bool IsPiece(string unit) =>
        unit.Contains("large") || unit.Contains("medium") || unit.Contains("small") ||
        unit.Contains("whole") || unit.Contains("each") || unit.Contains("piece") ||
        unit.Contains("clove") || unit.Contains("unit");
}
