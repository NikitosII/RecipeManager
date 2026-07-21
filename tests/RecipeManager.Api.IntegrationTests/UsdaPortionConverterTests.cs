using RecipeManager.Infrastructure.Services;

namespace RecipeManager.Api.IntegrationTests;

// Pure-logic tests for the USDA portion -> conversion-hint derivation. No container needed.
public class UsdaPortionConverterTests
{
    [Fact]
    public void Derive_FromVolumePortions_ComputesAverageDensity()
    {
        // Real olive-oil portions: 1 tbsp = 14 g, 1 tsp = 4.5 g.
        var (density, gramsPerPiece) = UsdaPortionConverter.Derive(
        [
            new UsdaPortion(1m, 14m, "tablespoon", "undetermined"),
            new UsdaPortion(1m, 4.5m, "teaspoon", "undetermined"),
        ]);

        // (14/15 + 4.5/5) / 2 = 0.91667.
        Assert.Equal(0.9167m, density);
        Assert.Null(gramsPerPiece);
    }

    [Fact]
    public void Derive_UsesMeasureUnitNameWhenMeaningful()
    {
        var (density, _) = UsdaPortionConverter.Derive(
        [
            new UsdaPortion(1m, 120m, null, "cup"), // 120 g / 240 ml = 0.5
        ]);

        Assert.Equal(0.5m, density);
    }

    [Fact]
    public void Derive_FromCountedPortion_ComputesGramsPerPiece()
    {
        var (density, gramsPerPiece) = UsdaPortionConverter.Derive(
        [
            new UsdaPortion(1m, 50m, "large", "undetermined"),
        ]);

        Assert.Null(density);
        Assert.Equal(50m, gramsPerPiece);
    }

    [Fact]
    public void Derive_PrefersMediumPieceOverOthers()
    {
        var (_, gramsPerPiece) = UsdaPortionConverter.Derive(
        [
            new UsdaPortion(1m, 60m, "large", "undetermined"),
            new UsdaPortion(1m, 50m, "medium", "undetermined"),
        ]);

        Assert.Equal(50m, gramsPerPiece);
    }

    [Fact]
    public void Derive_IgnoresNonPositiveOrUnrecognizedPortions()
    {
        var (density, gramsPerPiece) = UsdaPortionConverter.Derive(
        [
            new UsdaPortion(1m, 0m, "tablespoon", null),   // zero weight -> ignored
            new UsdaPortion(0m, 14m, "tablespoon", null),  // zero amount -> ignored
            new UsdaPortion(1m, 30m, "serving", null),     // not a volume or a piece
            new UsdaPortion(1m, 15m, "tablespoon", null),  // the only usable one
        ]);

        Assert.Equal(1m, density); // 15 g / 15 ml
        Assert.Null(gramsPerPiece);
    }

    [Fact]
    public void Derive_WithNoPortions_ReturnsNothing()
    {
        var (density, gramsPerPiece) = UsdaPortionConverter.Derive([]);

        Assert.Null(density);
        Assert.Null(gramsPerPiece);
    }
}
