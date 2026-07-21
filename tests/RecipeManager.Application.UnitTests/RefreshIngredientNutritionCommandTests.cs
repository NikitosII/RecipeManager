using NSubstitute;
using RecipeManager.Application.Features.Ingredients.Commands;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class RefreshIngredientNutritionCommandTests
{
    private readonly IIngredientRepository _ingredients = Substitute.For<IIngredientRepository>();
    private readonly INutritionProvider _nutrition = Substitute.For<INutritionProvider>();

    private RefreshIngredientNutritionCommandHandler CreateHandler() => new(_ingredients, _nutrition);

    [Fact]
    public async Task Handle_LookupHit_CachesMacrosAndReturnsThem()
    {
        var ingredient = new Ingredient("Flour"); // no cached macros yet
        _ingredients.GetByIdAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(ingredient);
        _nutrition.LookupAsync("Flour", Arg.Any<CancellationToken>())
            .Returns(new NutritionLookup(364m, 10m, 1m, 76m, 2.7m));

        var result = await CreateHandler().Handle(new RefreshIngredientNutritionCommand(ingredient.Id), CancellationToken.None);

        Assert.True(result.HasNutrition);
        Assert.Equal(364m, result.CaloriesPer100g);
        Assert.Equal(2.7m, result.FiberPer100g);
        Assert.True(ingredient.HasNutrition);
        _ingredients.Received(1).Update(ingredient);
        await _ingredients.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LookupMiss_LeavesIngredientUnchangedAndDoesNotSave()
    {
        var ingredient = new Ingredient("Moon dust");
        _ingredients.GetByIdAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(ingredient);
        _nutrition.LookupAsync("Moon dust", Arg.Any<CancellationToken>()).Returns((NutritionLookup?)null);

        var result = await CreateHandler().Handle(new RefreshIngredientNutritionCommand(ingredient.Id), CancellationToken.None);

        Assert.False(result.HasNutrition);
        Assert.Null(result.CaloriesPer100g);
        _ingredients.DidNotReceive().Update(Arg.Any<Ingredient>());
        await _ingredients.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingIngredient_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _ingredients.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new RefreshIngredientNutritionCommand(id), CancellationToken.None));

        await _nutrition.DidNotReceive().LookupAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
