namespace RecipeManager.Domain.Enums;

/// <summary>
/// Whether a recipe's nutrition figures are computed automatically from its
/// ingredients (<see cref="Auto"/>) or entered by the author (<see cref="Manual"/>).
/// </summary>
public enum NutritionMode
{
    Auto = 1,
    Manual = 2
}
