using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Domain.UnitTests;

public class RecipeStepTests
{
    private static Recipe NewRecipe() =>
        new("Pancakes", "Fluffy", DifficultyLevel.Easy, 10, 15, 4, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void AppendStep_NumbersStepsSequentiallyFromOne()
    {
        var recipe = NewRecipe();

        recipe.AppendStep("Mix flour");
        recipe.AppendStep("Add eggs");
        recipe.AppendStep("Fry");

        Assert.Equal(new[] { 1, 2, 3 }, recipe.GetSteps().Select(s => s.StepNumber));
        Assert.Equal(new[] { "Mix flour", "Add eggs", "Fry" }, recipe.GetSteps().Select(s => s.Description));
    }

    [Fact]
    public void AppendStep_ReturnsTheCreatedStep_WithCorrectNumber()
    {
        var recipe = NewRecipe();
        recipe.AppendStep("First");

        var second = recipe.AppendStep("Second");

        Assert.Equal(2, second.StepNumber);
        Assert.Equal("Second", second.Description);
    }

    [Fact]
    public void InsertStepAfter_ShiftsSubsequentStepsUp()
    {
        var recipe = NewRecipe();
        recipe.AppendStep("A"); // 1
        recipe.AppendStep("B"); // 2
        recipe.AppendStep("C"); // 3

        var inserted = recipe.InsertStepAfter(1, "A.5");

        Assert.Equal(2, inserted.StepNumber);
        var ordered = recipe.GetSteps().ToList();
        Assert.Equal(new[] { 1, 2, 3, 4 }, ordered.Select(s => s.StepNumber));
        Assert.Equal(new[] { "A", "A.5", "B", "C" }, ordered.Select(s => s.Description));
    }

    [Fact]
    public void InsertStepAfter_AtEnd_AppendsWithoutShifting()
    {
        var recipe = NewRecipe();
        recipe.AppendStep("A"); // 1
        recipe.AppendStep("B"); // 2

        var inserted = recipe.InsertStepAfter(2, "C");

        Assert.Equal(3, inserted.StepNumber);
        Assert.Equal(new[] { "A", "B", "C" }, recipe.GetSteps().Select(s => s.Description));
    }

    [Fact]
    public void InsertStepAfter_UnknownAnchor_ThrowsNotFound()
    {
        var recipe = NewRecipe();
        recipe.AppendStep("A");

        Assert.Throws<NotFoundException>(() => recipe.InsertStepAfter(99, "nope"));
    }

    [Fact]
    public void RemoveStep_ClosesTheNumberingGap()
    {
        var recipe = NewRecipe();
        recipe.AppendStep("A"); // 1
        recipe.AppendStep("B"); // 2
        recipe.AppendStep("C"); // 3

        recipe.RemoveStep(2);

        var ordered = recipe.GetSteps().ToList();
        Assert.Equal(new[] { 1, 2 }, ordered.Select(s => s.StepNumber));
        Assert.Equal(new[] { "A", "C" }, ordered.Select(s => s.Description));
    }

    [Fact]
    public void RemoveStep_UnknownNumber_ThrowsNotFound()
    {
        var recipe = NewRecipe();
        recipe.AppendStep("A");

        Assert.Throws<NotFoundException>(() => recipe.RemoveStep(42));
    }
}
