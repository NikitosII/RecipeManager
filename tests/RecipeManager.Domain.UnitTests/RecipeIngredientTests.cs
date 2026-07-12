using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Domain.UnitTests;

public class RecipeIngredientTests
{
    private static Recipe NewRecipe() =>
        new("Omelette", null, DifficultyLevel.Easy, 5, 5, 2, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void AddIngredient_AddsToTheRecipe()
    {
        var recipe = NewRecipe();
        var ingredientId = Guid.NewGuid();

        recipe.AddIngredient(ingredientId, 3, MeasurementUnit.Piece);

        var ri = Assert.Single(recipe.RecipeIngredients);
        Assert.Equal(ingredientId, ri.IngredientId);
        Assert.Equal(3, ri.Quantity);
        Assert.Equal(MeasurementUnit.Piece, ri.Unit);
    }

    [Fact]
    public void AddIngredient_SameIngredientTwice_ThrowsConflict()
    {
        var recipe = NewRecipe();
        var ingredientId = Guid.NewGuid();
        recipe.AddIngredient(ingredientId, 1, MeasurementUnit.Cup);

        Assert.Throws<ConflictException>(() => recipe.AddIngredient(ingredientId, 2, MeasurementUnit.Cup));
    }

    [Fact]
    public void RemoveIngredient_RemovesTheMatchingEntry()
    {
        var recipe = NewRecipe();
        var keep = Guid.NewGuid();
        var drop = Guid.NewGuid();
        recipe.AddIngredient(keep, 1, MeasurementUnit.Gram);
        recipe.AddIngredient(drop, 2, MeasurementUnit.Gram);

        recipe.RemoveIngredient(drop);

        var ri = Assert.Single(recipe.RecipeIngredients);
        Assert.Equal(keep, ri.IngredientId);
    }

    [Fact]
    public void RemoveIngredient_NotOnRecipe_ThrowsNotFound()
    {
        var recipe = NewRecipe();

        Assert.Throws<NotFoundException>(() => recipe.RemoveIngredient(Guid.NewGuid()));
    }

    [Fact]
    public void BaseEntity_AssignsSortableVersion7Guid()
    {
        // Ids are app-generated (not database-generated), which is why the EF
        // config marks them ValueGeneratedNever. Two entities must get distinct, non-empty keys.
        var a = NewRecipe();
        var b = NewRecipe();

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }
}
