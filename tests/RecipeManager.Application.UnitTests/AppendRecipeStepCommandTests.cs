using NSubstitute;
using RecipeManager.Application.Features.Recipes.Commands;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class AppendRecipeStepCommandTests
{
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private AppendRecipeStepCommandHandler CreateHandler() => new(_recipes);

    private static Recipe NewRecipe() =>
        new("Toast", null, DifficultyLevel.Easy, 1, 2, 1, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Handle_AppendsStepAndSaves()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var command = new AppendRecipeStepCommand(recipe.Id, "  Butter the bread  ", recipe.UserId);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal(1, result.StepNumber);
        Assert.Equal("Butter the bread", result.Description); // trimmed
        await _recipes.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Single(recipe.Steps);
    }

    [Fact]
    public async Task Handle_BlankDescription_ThrowsValidation()
    {
        var command = new AppendRecipeStepCommand(Guid.NewGuid(), "   ", Guid.NewGuid());

        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MissingRecipe_ThrowsNotFound()
    {
        var recipeId = Guid.NewGuid();
        _recipes.GetByIdWithDetailsAsync(recipeId, Arg.Any<CancellationToken>()).Returns((Recipe?)null);

        var command = new AppendRecipeStepCommand(recipeId, "Do a thing", Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RequestingUserIsNotOwner_ThrowsForbidden()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var command = new AppendRecipeStepCommand(recipe.Id, "Do a thing", Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, CancellationToken.None));
        await _recipes.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
