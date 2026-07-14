using NSubstitute;
using RecipeManager.Application.Features.Favorites.Commands;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class FavoriteCommandTests
{
    private readonly IFavoriteRepository _favorites = Substitute.For<IFavoriteRepository>();
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private static Recipe NewRecipe() =>
        new("Stew", null, DifficultyLevel.Medium, 15, 60, 6, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Add_WhenRecipeMissing_ThrowsNotFound()
    {
        var recipeId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipeId, Arg.Any<CancellationToken>()).Returns((Recipe?)null);

        var handler = new AddFavoriteCommandHandler(_favorites, _recipes);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new AddFavoriteCommand(recipeId, Guid.NewGuid()), CancellationToken.None));
        await _favorites.DidNotReceive().AddAsync(Arg.Any<Favorite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_WhenAlreadyFavorited_IsNoOp()
    {
        var recipe = NewRecipe();
        var userId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _favorites.ExistsAsync(userId, recipe.Id, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new AddFavoriteCommandHandler(_favorites, _recipes);
        await handler.Handle(new AddFavoriteCommand(recipe.Id, userId), CancellationToken.None);

        await _favorites.DidNotReceive().AddAsync(Arg.Any<Favorite>(), Arg.Any<CancellationToken>());
        await _favorites.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_WhenNew_AddsFavoriteAndSaves()
    {
        var recipe = NewRecipe();
        var userId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _favorites.ExistsAsync(userId, recipe.Id, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new AddFavoriteCommandHandler(_favorites, _recipes);
        await handler.Handle(new AddFavoriteCommand(recipe.Id, userId), CancellationToken.None);

        await _favorites.Received(1).AddAsync(
            Arg.Is<Favorite>(f => f.UserId == userId && f.RecipeId == recipe.Id), Arg.Any<CancellationToken>());
        await _favorites.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remove_WhenFavorited_DeletesAndSaves()
    {
        var userId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var favorite = new Favorite(userId, recipeId);
        _favorites.GetAsync(userId, recipeId, Arg.Any<CancellationToken>()).Returns(favorite);

        var handler = new RemoveFavoriteCommandHandler(_favorites);
        await handler.Handle(new RemoveFavoriteCommand(recipeId, userId), CancellationToken.None);

        _favorites.Received(1).Delete(favorite);
        await _favorites.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remove_WhenNotFavorited_IsNoOp()
    {
        var userId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        _favorites.GetAsync(userId, recipeId, Arg.Any<CancellationToken>()).Returns((Favorite?)null);

        var handler = new RemoveFavoriteCommandHandler(_favorites);
        await handler.Handle(new RemoveFavoriteCommand(recipeId, userId), CancellationToken.None);

        _favorites.DidNotReceive().Delete(Arg.Any<Favorite>());
        await _favorites.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
