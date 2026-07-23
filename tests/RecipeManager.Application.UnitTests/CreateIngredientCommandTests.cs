using NSubstitute;
using RecipeManager.Application.Features.Ingredients.Commands;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class CreateIngredientCommandTests
{
    private readonly IIngredientRepository _ingredients = Substitute.For<IIngredientRepository>();
    private readonly IIngredientEnrichmentQueue _enrichment = Substitute.For<IIngredientEnrichmentQueue>();

    private CreateIngredientCommandHandler CreateHandler() => new(_ingredients, _enrichment);

    [Fact]
    public async Task Handle_NewIngredient_SavesThenQueuesEnrichment()
    {
        _ingredients.GetByNameAsync("Flour", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        Ingredient? added = null;
        await _ingredients.AddAsync(Arg.Do<Ingredient>(i => added = i), Arg.Any<CancellationToken>());

        var id = await CreateHandler().Handle(new CreateIngredientCommand("Flour"), CancellationToken.None);

        Assert.NotNull(added);
        // Nutrition is looked up off the write path, so it isn't set synchronously here.
        Assert.False(added!.HasNutrition);
        await _ingredients.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _enrichment.Received(1).Enqueue(id);
    }

    [Fact]
    public async Task Handle_QueuesEnrichmentOnlyAfterTheRowIsSaved()
    {
        _ingredients.GetByNameAsync("Basil", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        // Enqueue must happen after SaveChanges, so the worker can load the row in its scope.
        var savedBeforeEnqueue = false;
        _enrichment.When(q => q.Enqueue(Arg.Any<Guid>()))
            .Do(_ => savedBeforeEnqueue = _ingredients.ReceivedCalls()
                .Any(c => c.GetMethodInfo().Name == nameof(IIngredientRepository.SaveChangesAsync)));

        await CreateHandler().Handle(new CreateIngredientCommand("Basil"), CancellationToken.None);

        Assert.True(savedBeforeEnqueue);
    }

    [Fact]
    public async Task Handle_TrimsNameBeforeCreation()
    {
        _ingredients.GetByNameAsync("Basil", Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        Ingredient? added = null;
        await _ingredients.AddAsync(Arg.Do<Ingredient>(i => added = i), Arg.Any<CancellationToken>());

        await CreateHandler().Handle(new CreateIngredientCommand("  Basil  "), CancellationToken.None);

        Assert.Equal("Basil", added!.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_BlankName_ThrowsValidation(string name)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => CreateHandler().Handle(new CreateIngredientCommand(name), CancellationToken.None));

        _enrichment.DidNotReceive().Enqueue(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsConflict()
    {
        _ingredients.GetByNameAsync("Salt", Arg.Any<CancellationToken>()).Returns(new Ingredient("Salt"));

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateHandler().Handle(new CreateIngredientCommand("Salt"), CancellationToken.None));

        await _ingredients.DidNotReceive().AddAsync(Arg.Any<Ingredient>(), Arg.Any<CancellationToken>());
        _enrichment.DidNotReceive().Enqueue(Arg.Any<Guid>());
    }
}
