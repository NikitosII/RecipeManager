namespace RecipeManager.Application.Configuration;

/// <summary>
/// Configures the external nutrition source (USDA FoodData Central).
/// </summary>
public class NutritionOptions
{
    public const string SectionName = "Nutrition";

    /// <summary>When false, ingredient nutrition is never looked up and stays empty.</summary>
    public bool Enabled { get; init; } = true;

    public string BaseUrl { get; init; } = "https://api.nal.usda.gov/fdc/v1/";

    public string UsdaApiKey { get; init; } = "DEMO_KEY";
}
