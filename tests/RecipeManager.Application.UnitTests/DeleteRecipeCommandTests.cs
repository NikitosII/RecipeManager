using NSubstitute;
using RecipeManager.Application.Features.Recipes.Commands;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class DeleteRecipeCommandTests
{
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private DeleteRecipeCommandHandler CreateHandler() => new(_recipes);

    private static Recipe NewRecipe() =>
        new("Stew", null, DifficultyLevel.Medium, 15, 60, 6, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Handle_OwnerDeletes_RemovesRecipeAndSaves()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var command = new DeleteRecipeCommand(recipe.Id, recipe.UserId);
        await CreateHandler().Handle(command, CancellationToken.None);

        _recipes.Received(1).Delete(recipe);
        await _recipes.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonOwner_ThrowsForbiddenAndDoesNotDelete()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var command = new DeleteRecipeCommand(recipe.Id, Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, CancellationToken.None));
        _recipes.DidNotReceive().Delete(Arg.Any<Recipe>());
        await _recipes.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingRecipe_ThrowsNotFound()
    {
        var recipeId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipeId, Arg.Any<CancellationToken>()).Returns((Recipe?)null);

        var command = new DeleteRecipeCommand(recipeId, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }
}
