using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeManager.Application.Configuration;
using RecipeManager.Application.Interfaces;

namespace RecipeManager.Infrastructure.Services;

/// <summary>
/// Looks up nutrition from USDA FoodData Central. The search endpoint gives per-100g
/// macros; a follow-up detail call gives portion data, from which we derive density
/// (for volume units) and grams-per-piece (for counted units).
/// </summary>
public class UsdaNutritionProvider(HttpClient httpClient, IOptions<NutritionOptions> options, ILogger<UsdaNutritionProvider> logger) : INutritionProvider
{
    private readonly NutritionOptions _options = options.Value;
    private const string EnergyKcal = "208";
    private const string Protein = "203";
    private const string Fat = "204";
    private const string Carbohydrate = "205";
    private const string Fiber = "291";

    public async Task<NutritionLookup?> LookupAsync(string ingredientName, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(ingredientName))
            return null;

        // Restrict to generic per-100g reference foods for clean, comparable macros.
        var url = "foods/search" +
            $"?api_key={Uri.EscapeDataString(_options.UsdaApiKey)}" +
            $"&query={Uri.EscapeDataString(ingredientName)}" +
            "&pageSize=1" +
            $"&dataType={Uri.EscapeDataString("Foundation,SR Legacy")}";

        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "USDA nutrition lookup for '{Ingredient}' returned {StatusCode}.",
                    ingredientName, (int)response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken);
            var food = payload?.Foods?.FirstOrDefault();
            if (food?.FoodNutrients is not { Count: > 0 } nutrients)
                return null;

            decimal? Value(string number, string? unit = null) => nutrients
                .FirstOrDefault(n => n.NutrientNumber == number && (unit is null || n.UnitName == unit))?.Value;

            var calories = Value(EnergyKcal, "KCAL");
            var protein = Value(Protein);
            var fat = Value(Fat);
            var carbs = Value(Carbohydrate);

            // Without all four core macros the calculator can't use the ingredient, so
            // don't cache a partial record — leave it empty to retry or fill in manually.
            if (calories is null || protein is null || fat is null || carbs is null)
                return null;

            var (density, gramsPerPiece) = food.FdcId is { } fdcId
                ? await TryDeriveConversionAsync(fdcId, ingredientName, cancellationToken)
                : (null, null);

            return new NutritionLookup(
                calories.Value, protein.Value, fat.Value, carbs.Value, Value(Fiber), density, gramsPerPiece);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "USDA nutrition lookup failed for '{Ingredient}'.", ingredientName);
            return null;
        }
    }

    /// <summary>
    /// Fetches the food's portion list and derives unit-conversion hints.
    /// </summary>
    private async Task<(decimal? Density, decimal? GramsPerPiece)> TryDeriveConversionAsync(
        long fdcId, string ingredientName, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"food/{fdcId}?api_key={Uri.EscapeDataString(_options.UsdaApiKey)}";
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (null, null);

            var detail = await response.Content.ReadFromJsonAsync<DetailResponse>(cancellationToken);
            var portions = detail?.FoodPortions?
                .Select(p => new UsdaPortion(p.Amount, p.GramWeight, p.Modifier, p.MeasureUnit?.Name))
                .ToList();

            return UsdaPortionConverter.Derive(portions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogDebug(ex, "USDA portion lookup failed for '{Ingredient}'.", ingredientName);
            return (null, null);
        }
    }

    // Minimal shape of the foods/search response; deserialized case-insensitively.
    private sealed record SearchResponse(List<SearchFood>? Foods);
    private sealed record SearchFood(long? FdcId, List<SearchNutrient>? FoodNutrients);
    private sealed record SearchNutrient(string? NutrientNumber, string? UnitName, decimal? Value);

    // Minimal shape of the food/{id} detail response (only the portion list).
    private sealed record DetailResponse(List<DetailPortion>? FoodPortions);
    private sealed record DetailPortion(decimal? Amount, decimal? GramWeight, string? Modifier, DetailMeasureUnit? MeasureUnit);
    private sealed record DetailMeasureUnit(string? Name);
}
