namespace RecipeManager.Domain.Nutrition;

/// <summary>
/// A set of macronutrient figures. Calories are in kcal; protein, fat,
/// carbohydrates and fibre are in grams. Depending on context these represent
/// per-serving totals (computed by <see cref="NutritionCalculator"/>) or the
/// author's manually entered values.
/// </summary>
public sealed record NutritionFacts(
    decimal Calories,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrates,
    decimal? Fiber)
{
    public static readonly NutritionFacts Zero = new(0m, 0m, 0m, 0m, null);
}
