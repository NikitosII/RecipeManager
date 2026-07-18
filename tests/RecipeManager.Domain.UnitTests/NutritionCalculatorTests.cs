using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Nutrition;

namespace RecipeManager.Domain.UnitTests;

public class NutritionCalculatorTests
{
    // Builds a line with sensible defaults (100 kcal / 10 P / 5 F / 20 C per 100 g)
    // so each test only overrides what it cares about.
    private static NutritionLine Line(
        decimal quantity,
        MeasurementUnit unit,
        decimal? calories = 100m,
        decimal? protein = 10m,
        decimal? fat = 5m,
        decimal? carbs = 20m,
        decimal? fiber = null,
        decimal? density = null,
        decimal? gramsPerPiece = null,
        string name = "test")
        => new(name, quantity, unit, calories, protein, fat, carbs, fiber, density, gramsPerPiece);

    [Fact]
    public void Calculate_SumsGramLinesAndDividesByServings()
    {
        // 200 g at the default per-100g figures -> factor 2 -> 200/20/10/40 total, /2 servings.
        var result = NutritionCalculator.Calculate(2, [Line(200m, MeasurementUnit.Gram)]);

        Assert.Equal(100m, result.PerServing.Calories);
        Assert.Equal(10m, result.PerServing.Protein);
        Assert.Equal(5m, result.PerServing.Fat);
        Assert.Equal(20m, result.PerServing.Carbohydrates);
        Assert.True(result.IsComplete);
        Assert.True(result.HasAnyData);
        Assert.Equal(1, result.CountedCount);
        Assert.Equal(1, result.TotalCount);
        Assert.Empty(result.Uncounted);
    }

    [Fact]
    public void Calculate_ConvertsKilogramsToGrams()
    {
        // 1 kg = 1000 g -> factor 10 -> 50 kcal/100g becomes 500 kcal.
        var result = NutritionCalculator.Calculate(1, [Line(1m, MeasurementUnit.Kilogram, calories: 50m)]);

        Assert.Equal(500m, result.PerServing.Calories);
    }

    [Fact]
    public void Calculate_ConvertsVolumeUsingDensity()
    {
        // 1 cup = 240 ml, density 0.5 g/ml -> 120 g -> factor 1.2 -> 120 kcal.
        var result = NutritionCalculator.Calculate(
            1, [Line(1m, MeasurementUnit.Cup, calories: 100m, density: 0.5m)]);

        Assert.Equal(120m, result.PerServing.Calories);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public void Calculate_ConvertsPiecesUsingGramsPerPiece()
    {
        // 3 pieces * 50 g = 150 g -> factor 1.5 -> 200 kcal/100g becomes 300 kcal.
        var result = NutritionCalculator.Calculate(
            1, [Line(3m, MeasurementUnit.Piece, calories: 200m, gramsPerPiece: 50m)]);

        Assert.Equal(300m, result.PerServing.Calories);
    }

    [Fact]
    public void Calculate_WithoutCoreMacros_ReportsMissingNutritionData()
    {
        var result = NutritionCalculator.Calculate(1, [Line(100m, MeasurementUnit.Gram, calories: null)]);

        Assert.Equal(0, result.CountedCount);
        Assert.False(result.HasAnyData);
        Assert.Equal(NutritionFacts.Zero, result.PerServing);
        var uncounted = Assert.Single(result.Uncounted);
        Assert.Equal(UncountedReason.MissingNutritionData, uncounted.Reason);
    }

    [Theory]
    [InlineData(MeasurementUnit.Piece)] // no grams-per-piece hint
    [InlineData(MeasurementUnit.Cup)]   // no density hint
    [InlineData(MeasurementUnit.Milliliter)]
    [InlineData(MeasurementUnit.Pinch)] // never convertible
    public void Calculate_WhenUnitCannotConvertToGrams_ReportsUnconvertibleUnit(MeasurementUnit unit)
    {
        var result = NutritionCalculator.Calculate(1, [Line(1m, unit)]);

        Assert.Equal(0, result.CountedCount);
        var uncounted = Assert.Single(result.Uncounted);
        Assert.Equal(UncountedReason.UnconvertibleUnit, uncounted.Reason);
    }

    [Fact]
    public void Calculate_WithPartialCoverage_CountsOnlyUsableLinesButKeepsTheTotal()
    {
        var result = NutritionCalculator.Calculate(1,
        [
            Line(100m, MeasurementUnit.Gram, name: "flour"),
            Line(1m, MeasurementUnit.Piece, name: "egg"), // no grams-per-piece -> uncounted
        ]);

        Assert.Equal(1, result.CountedCount);
        Assert.Equal(2, result.TotalCount);
        Assert.False(result.IsComplete);
        Assert.True(result.HasAnyData);
        var uncounted = Assert.Single(result.Uncounted);
        Assert.Equal("egg", uncounted.Name);
    }

    [Fact]
    public void Calculate_SumsFiberOnlyFromLinesThatHaveIt()
    {
        var result = NutritionCalculator.Calculate(1,
        [
            Line(100m, MeasurementUnit.Gram, fiber: 5m),
            Line(100m, MeasurementUnit.Gram, fiber: null),
        ]);

        Assert.Equal(5m, result.PerServing.Fiber);
    }

    [Fact]
    public void Calculate_WhenNoLineHasFiber_LeavesFiberNull()
    {
        var result = NutritionCalculator.Calculate(1, [Line(100m, MeasurementUnit.Gram)]);

        Assert.Null(result.PerServing.Fiber);
    }

    [Fact]
    public void Calculate_WithNoIngredients_ReturnsZeroAndNoData()
    {
        var result = NutritionCalculator.Calculate(4, []);

        Assert.Equal(NutritionFacts.Zero, result.PerServing);
        Assert.False(result.IsComplete);
        Assert.False(result.HasAnyData);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void Calculate_RoundsPerServingToOneDecimal()
    {
        // 100 g -> 100 kcal total, split across 3 servings -> 33.33... -> 33.3.
        var result = NutritionCalculator.Calculate(3, [Line(100m, MeasurementUnit.Gram, calories: 100m)]);

        Assert.Equal(33.3m, result.PerServing.Calories);
    }

    [Fact]
    public void Calculate_WhenServingsIsZero_DoesNotDivideByZero()
    {
        // Servings is validated upstream; the calculator guards defensively by treating 0 as 1.
        var result = NutritionCalculator.Calculate(0, [Line(100m, MeasurementUnit.Gram, calories: 100m)]);

        Assert.Equal(100m, result.PerServing.Calories);
    }
}
