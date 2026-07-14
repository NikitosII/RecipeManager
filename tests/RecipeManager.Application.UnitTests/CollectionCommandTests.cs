using NSubstitute;
using RecipeManager.Application.Features.Collections.Commands;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class CollectionCommandTests
{
    private readonly ICollectionRepository _collections = Substitute.For<ICollectionRepository>();
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private static Recipe NewRecipe() =>
        new("Stew", null, DifficultyLevel.Medium, 15, 60, 6, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Create_WithBlankName_ThrowsValidation()
    {
        var handler = new CreateCollectionCommandHandler(_collections);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new CreateCollectionCommand("  ", null, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Create_Valid_AddsCollectionAndReturnsId()
    {
        var userId = Guid.NewGuid();
        var handler = new CreateCollectionCommandHandler(_collections);

        var id = await handler.Handle(
            new CreateCollectionCommand("Weeknight dinners", "Fast", userId), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        await _collections.Received(1).AddAsync(
            Arg.Is<Collection>(c => c.Name == "Weeknight dinners" && c.UserId == userId), Arg.Any<CancellationToken>());
        await _collections.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ByNonOwner_ThrowsForbidden()
    {
        var collection = new Collection("Baking", null, Guid.NewGuid());
        _collections.GetByIdAsync(collection.Id, Arg.Any<CancellationToken>()).Returns(collection);

        var handler = new UpdateCollectionCommandHandler(_collections);
        var command = new UpdateCollectionCommand(collection.Id, "New name", null, Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        await _collections.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_MissingCollection_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _collections.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Collection?)null);

        var handler = new DeleteCollectionCommandHandler(_collections);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteCollectionCommand(id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task AddRecipe_ByOwner_AddsMembershipAndSaves()
    {
        var owner = Guid.NewGuid();
        var collection = new Collection("Baking", null, owner);
        var recipe = NewRecipe();
        _collections.GetByIdAsync(collection.Id, Arg.Any<CancellationToken>()).Returns(collection);
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        var handler = new AddRecipeToCollectionCommandHandler(_collections, _recipes);
        await handler.Handle(new AddRecipeToCollectionCommand(collection.Id, recipe.Id, owner), CancellationToken.None);

        Assert.Contains(collection.Recipes, r => r.RecipeId == recipe.Id);
        await _collections.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddRecipe_ByNonOwner_ThrowsForbidden()
    {
        var collection = new Collection("Baking", null, Guid.NewGuid());
        _collections.GetByIdAsync(collection.Id, Arg.Any<CancellationToken>()).Returns(collection);

        var handler = new AddRecipeToCollectionCommandHandler(_collections, _recipes);
        var command = new AddRecipeToCollectionCommand(collection.Id, Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        await _collections.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveRecipe_ByOwner_RemovesMembershipAndSaves()
    {
        var owner = Guid.NewGuid();
        var collection = new Collection("Baking", null, owner);
        var recipeId = Guid.NewGuid();
        collection.AddRecipe(recipeId);
        _collections.GetByIdAsync(collection.Id, Arg.Any<CancellationToken>()).Returns(collection);

        var handler = new RemoveRecipeFromCollectionCommandHandler(_collections);
        await handler.Handle(new RemoveRecipeFromCollectionCommand(collection.Id, recipeId, owner), CancellationToken.None);

        Assert.DoesNotContain(collection.Recipes, r => r.RecipeId == recipeId);
        await _collections.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
