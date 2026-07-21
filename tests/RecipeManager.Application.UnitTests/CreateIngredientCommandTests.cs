using NSubstitute;
using RecipeManager.Application.Features.Ingredients.Commands;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class CreateIngredientCommandTests
{
    private readonly IIngredientRepository _ingredients = Substitute.For<IIngredientRepository>();
    private readonly INutritionProvider _nutrition = Substitute.For<INutritionProvider>();

    private CreateIngredientCommandHandler CreateHandler() => new(_ingredients, _nutrition);

    [Fact]
    public async Task Handle_NewIngredient_CachesNutritionFromTheProvider()
    {
        _ingredients.GetByNameAsync("Flour", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);
        _nutrition.LookupAsync("Flour", Arg.Any<CancellationToken>())
            .Returns(new NutritionLookup(364m, 10m, 1m, 76m, 2.7m));

        Ingredient? added = null;
        await _ingredients.AddAsync(Arg.Do<Ingredient>(i => added = i), Arg.Any<CancellationToken>());

        await CreateHandler().Handle(new CreateIngredientCommand("Flour"), CancellationToken.None);

        Assert.NotNull(added);
        Assert.True(added!.HasNutrition);
        Assert.Equal(364m, added.CaloriesPer100g);
        Assert.Equal(2.7m, added.FiberPer100g);
        await _ingredients.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProviderReturnsNull_StillCreatesIngredientWithoutNutrition()
    {
        _ingredients.GetByNameAsync("Moon dust", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);
        _nutrition.LookupAsync("Moon dust", Arg.Any<CancellationToken>()).Returns((NutritionLookup?)null);

        Ingredient? added = null;
        await _ingredients.AddAsync(Arg.Do<Ingredient>(i => added = i), Arg.Any<CancellationToken>());

        await CreateHandler().Handle(new CreateIngredientCommand("Moon dust"), CancellationToken.None);

        Assert.NotNull(added);
        Assert.False(added!.HasNutrition);
        await _ingredients.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProviderReturnsConversionHints_CachesThem()
    {
        _ingredients.GetByNameAsync("Olive oil", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);
        _nutrition.LookupAsync("Olive oil", Arg.Any<CancellationToken>())
            .Returns(new NutritionLookup(884m, 0m, 100m, 0m, 0m, DensityGramsPerMl: 0.92m, GramsPerPiece: null));

        Ingredient? added = null;
        await _ingredients.AddAsync(Arg.Do<Ingredient>(i => added = i), Arg.Any<CancellationToken>());

        await CreateHandler().Handle(new CreateIngredientCommand("Olive oil"), CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(0.92m, added!.DensityGramsPerMl);
        Assert.Null(added.GramsPerPiece);
    }

    [Fact]
    public async Task Handle_TrimsNameBeforeLookupAndCreation()
    {
        _ingredients.GetByNameAsync("Basil", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        Ingredient? added = null;
        await _ingredients.AddAsync(Arg.Do<Ingredient>(i => added = i), Arg.Any<CancellationToken>());

        await CreateHandler().Handle(new CreateIngredientCommand("  Basil  "), CancellationToken.None);

        Assert.Equal("Basil", added!.Name);
        await _nutrition.Received(1).LookupAsync("Basil", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_BlankName_ThrowsValidation(string name)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => CreateHandler().Handle(new CreateIngredientCommand(name), CancellationToken.None));

        await _nutrition.DidNotReceive().LookupAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsConflict()
    {
        _ingredients.GetByNameAsync("Salt", Arg.Any<CancellationToken>()).Returns(new Ingredient("Salt"));

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateHandler().Handle(new CreateIngredientCommand("Salt"), CancellationToken.None));

        await _ingredients.DidNotReceive().AddAsync(Arg.Any<Ingredient>(), Arg.Any<CancellationToken>());
    }
}
