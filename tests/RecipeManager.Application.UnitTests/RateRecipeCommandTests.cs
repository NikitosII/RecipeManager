using NSubstitute;
using RecipeManager.Application.Features.Ratings.Commands;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class RateRecipeCommandTests
{
    private readonly IRatingRepository _ratings = Substitute.For<IRatingRepository>();
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private RateRecipeCommandHandler CreateHandler() => new(_ratings, _recipes);

    private static Recipe NewRecipe() =>
        new("Stew", null, DifficultyLevel.Medium, 15, 60, 6, Guid.NewGuid(), Guid.NewGuid());

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Rate_OutOfRange_ThrowsValidation(int value)
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RateRecipeCommand(Guid.NewGuid(), Guid.NewGuid(), value), CancellationToken.None));
        await _recipes.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rate_MissingRecipe_ThrowsNotFound()
    {
        var recipeId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipeId, Arg.Any<CancellationToken>()).Returns((Recipe?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new RateRecipeCommand(recipeId, Guid.NewGuid(), 4), CancellationToken.None));
    }

    [Fact]
    public async Task Rate_FirstTime_AddsRatingAndSaves()
    {
        var recipe = NewRecipe();
        var userId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _ratings.GetAsync(userId, recipe.Id, Arg.Any<CancellationToken>()).Returns((Rating?)null);

        await CreateHandler().Handle(new RateRecipeCommand(recipe.Id, userId, 5), CancellationToken.None);

        await _ratings.Received(1).AddAsync(
            Arg.Is<Rating>(r => r.UserId == userId && r.RecipeId == recipe.Id && r.Value == 5),
            Arg.Any<CancellationToken>());
        await _ratings.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rate_Again_UpdatesExistingRatingWithoutAdding()
    {
        var recipe = NewRecipe();
        var userId = Guid.NewGuid();
        var existing = new Rating(userId, recipe.Id, 2);
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _ratings.GetAsync(userId, recipe.Id, Arg.Any<CancellationToken>()).Returns(existing);

        await CreateHandler().Handle(new RateRecipeCommand(recipe.Id, userId, 4), CancellationToken.None);

        Assert.Equal(4, existing.Value);
        await _ratings.DidNotReceive().AddAsync(Arg.Any<Rating>(), Arg.Any<CancellationToken>());
        await _ratings.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remove_WhenRated_DeletesAndSaves()
    {
        var userId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var rating = new Rating(userId, recipeId, 3);
        _ratings.GetAsync(userId, recipeId, Arg.Any<CancellationToken>()).Returns(rating);

        var handler = new RemoveRecipeRatingCommandHandler(_ratings);
        await handler.Handle(new RemoveRecipeRatingCommand(recipeId, userId), CancellationToken.None);

        _ratings.Received(1).Delete(rating);
        await _ratings.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remove_WhenNotRated_IsNoOp()
    {
        var userId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        _ratings.GetAsync(userId, recipeId, Arg.Any<CancellationToken>()).Returns((Rating?)null);

        var handler = new RemoveRecipeRatingCommandHandler(_ratings);
        await handler.Handle(new RemoveRecipeRatingCommand(recipeId, userId), CancellationToken.None);

        _ratings.DidNotReceive().Delete(Arg.Any<Rating>());
        await _ratings.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
