namespace RecipeManager.Domain.Nutrition;

/// <summary>Why an ingredient could not be included in the automatic calculation.</summary>
public enum UncountedReason
{
    /// <summary>The ingredient has no cached per-100g nutrition data yet.</summary>
    MissingNutritionData = 1,

    /// <summary>The ingredient's measurement unit could not be converted to grams
    /// (e.g. a piece or volume with no weight/density hint, or a pinch).</summary>
    UnconvertibleUnit = 2
}

/// <summary>An ingredient that was left out of the automatic total, with the reason.</summary>
public sealed record UncountedIngredient(string Name, UncountedReason Reason);

/// <summary>
/// The result of computing a recipe's nutrition automatically: the per-serving
/// figures together with a coverage report describing how many of the recipe's
/// ingredients actually contributed.
/// </summary>
public sealed record RecipeNutrition(
    NutritionFacts PerServing,
    int CountedCount,
    int TotalCount,
    IReadOnlyList<UncountedIngredient> Uncounted)
{
    /// <summary>True when every ingredient contributed to the total.</summary>
    public bool IsComplete => TotalCount > 0 && CountedCount == TotalCount;

    /// <summary>True when at least one ingredient contributed, so the figures are meaningful.</summary>
    public bool HasAnyData => CountedCount > 0;
}
