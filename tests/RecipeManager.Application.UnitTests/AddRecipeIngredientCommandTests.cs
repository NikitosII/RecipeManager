using NSubstitute;
using RecipeManager.Application.Features.Recipes.Commands;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class AddRecipeIngredientCommandTests
{
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();
    private readonly IIngredientRepository _ingredients = Substitute.For<IIngredientRepository>();
    private readonly INutritionProvider _nutrition = Substitute.For<INutritionProvider>();

    private AddRecipeIngredientCommandHandler CreateHandler() => new(_recipes, _ingredients, _nutrition);

    private static Recipe NewRecipe() =>
        new("Soup", null, DifficultyLevel.Easy, 5, 20, 4, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Handle_ExistingIngredient_ReusesItWithoutCreating()
    {
        var recipe = NewRecipe();
        var existing = new Ingredient("Salt");
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _ingredients.GetByNameAsync("Salt", Arg.Any<CancellationToken>()).Returns(existing);

        var command = new AddRecipeIngredientCommand(recipe.Id, "Salt", 2, MeasurementUnit.Pinch, recipe.UserId);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal(existing.Id, result.IngredientId);
        Assert.Equal("Salt", result.IngredientName);
        Assert.Equal(2, result.Quantity);
        await _ingredients.DidNotReceive().AddAsync(Arg.Any<Ingredient>(), Arg.Any<CancellationToken>());
        await _recipes.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Single(recipe.RecipeIngredients);
    }

    [Fact]
    public async Task Handle_UnknownIngredient_CreatesItThenAttaches()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _ingredients.GetByNameAsync("Basil", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        var command = new AddRecipeIngredientCommand(recipe.Id, "  Basil  ", 1, MeasurementUnit.Tablespoon, recipe.UserId);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Name is trimmed before lookup/creation.
        await _ingredients.Received(1).AddAsync(Arg.Is<Ingredient>(i => i.Name == "Basil"), Arg.Any<CancellationToken>());
        Assert.Equal("Basil", result.IngredientName);
        Assert.Single(recipe.RecipeIngredients);
    }

    [Fact]
    public async Task Handle_NewIngredient_CachesNutritionFromTheProvider()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _ingredients.GetByNameAsync("Flour", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);
        _nutrition.LookupAsync("Flour", Arg.Any<CancellationToken>())
            .Returns(new NutritionLookup(364m, 10m, 1m, 76m, 2.7m));

        Ingredient? created = null;
        await _ingredients.AddAsync(Arg.Do<Ingredient>(i => created = i), Arg.Any<CancellationToken>());

        var command = new AddRecipeIngredientCommand(recipe.Id, "Flour", 100, MeasurementUnit.Gram, recipe.UserId);
        await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(created);
        Assert.True(created!.HasNutrition);
        Assert.Equal(364m, created.CaloriesPer100g);
        Assert.Equal(2.7m, created.FiberPer100g);
    }

    [Fact]
    public async Task Handle_ExistingIngredientWithoutNutrition_BackfillsFromTheProvider()
    {
        var recipe = NewRecipe();
        var existing = new Ingredient("Flour"); // created before nutrition existed -> no macros
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _ingredients.GetByNameAsync("Flour", Arg.Any<CancellationToken>()).Returns(existing);
        _nutrition.LookupAsync("Flour", Arg.Any<CancellationToken>())
            .Returns(new NutritionLookup(364m, 10m, 1m, 76m, 2.7m));

        var command = new AddRecipeIngredientCommand(recipe.Id, "Flour", 50, MeasurementUnit.Gram, recipe.UserId);
        await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(existing.HasNutrition);
        Assert.Equal(364m, existing.CaloriesPer100g);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_BlankName_ThrowsValidation(string name)
    {
        var command = new AddRecipeIngredientCommand(Guid.NewGuid(), name, 1, MeasurementUnit.Gram, Guid.NewGuid());

        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_NonPositiveQuantity_ThrowsValidation(int quantity)
    {
        var command = new AddRecipeIngredientCommand(Guid.NewGuid(), "Sugar", quantity, MeasurementUnit.Gram, Guid.NewGuid());

        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MissingRecipe_ThrowsNotFound()
    {
        var recipeId = Guid.NewGuid();
        _recipes.GetByIdWithDetailsAsync(recipeId, Arg.Any<CancellationToken>()).Returns((Recipe?)null);

        var command = new AddRecipeIngredientCommand(recipeId, "Sugar", 1, MeasurementUnit.Gram, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RequestingUserIsNotOwner_ThrowsForbidden()
    {
        var recipe = NewRecipe();
        _recipes.GetByIdWithDetailsAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);

        // A different user than the recipe's creator.
        var command = new AddRecipeIngredientCommand(recipe.Id, "Sugar", 1, MeasurementUnit.Gram, Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, CancellationToken.None));
        await _recipes.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
