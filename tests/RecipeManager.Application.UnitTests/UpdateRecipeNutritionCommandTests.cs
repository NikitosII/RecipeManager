using NSubstitute;
using RecipeManager.Application.Features.Recipes.Commands;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class UpdateRecipeNutritionCommandTests
{
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private UpdateRecipeNutritionCommandHandler CreateHandler() => new(_recipes);

    private static Recipe NewRecipe() =>
        new("Soup", null, DifficultyLevel.Easy, 5, 20, 4, Guid.NewGuid(), Guid.NewGuid());

    private static UpdateRecipeNutritionCommand ManualCommand(Recipe recipe) =>
        new(recipe.Id, NutritionMode.Manual, 250m, 12m, 8m, 30m, 3m, recipe.UserId);

    [Fact]
    public async Task Handle_ManualMode_StoresFiguresAndReturnsThem()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var result = await CreateHandler().Handle(ManualCommand(recipe), CancellationToken.None);

        Assert.Equal(NutritionMode.Manual, result.Mode);
        Assert.Equal(250m, result.Calories);
        Assert.Equal(12m, result.Protein);
        Assert.Equal(3m, result.Fiber);
        Assert.True(result.IsComplete);
        Assert.Equal(NutritionMode.Manual, recipe.NutritionMode);
        Assert.Equal(250m, recipe.ManualCalories);
        await _recipes.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ManualMode_MissingCoreMacro_ThrowsValidation()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        // Protein omitted -> invalid for manual mode.
        var command = new UpdateRecipeNutritionCommand(
            recipe.Id, NutritionMode.Manual, 250m, null, 8m, 30m, 3m, recipe.UserId);

        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command, CancellationToken.None));
        await _recipes.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ManualMode_NegativeMacro_ThrowsValidation()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var command = new UpdateRecipeNutritionCommand(
            recipe.Id, NutritionMode.Manual, 250m, 12m, -1m, 30m, 3m, recipe.UserId);

        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AutomaticMode_ClearsStoredManualFigures()
    {
        var recipe = NewRecipe();
        recipe.SetManualNutrition(250m, 12m, 8m, 30m, 3m); // start in manual mode
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var command = new UpdateRecipeNutritionCommand(
            recipe.Id, NutritionMode.Auto, null, null, null, null, null, recipe.UserId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal(NutritionMode.Auto, result.Mode);
        Assert.Equal(NutritionMode.Auto, recipe.NutritionMode);
        Assert.Null(recipe.ManualCalories);
        await _recipes.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RequestingUserIsNotOwner_ThrowsForbidden()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var command = new UpdateRecipeNutritionCommand(
            recipe.Id, NutritionMode.Manual, 250m, 12m, 8m, 30m, 3m, Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, CancellationToken.None));
        await _recipes.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingRecipe_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _recipes.GetByIdWithDetailsAsync(id, Arg.Any<CancellationToken>()).Returns((Recipe?)null);

        var command = new UpdateRecipeNutritionCommand(
            id, NutritionMode.Auto, null, null, null, null, null, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }
}
