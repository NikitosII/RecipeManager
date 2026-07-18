using NSubstitute;
using RecipeManager.Application.Features.Recipes.Queries;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class GetRecipesQueryTests
{
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();
    private readonly IFavoriteRepository _favorites = Substitute.For<IFavoriteRepository>();
    private readonly IRatingRepository _ratings = Substitute.For<IRatingRepository>();

    public GetRecipesQueryTests()
    {
        _recipes.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<RecipeFilter>(), Arg.Any<RecipeSortBy>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<Recipe>)new List<Recipe>(), 0));
        _recipes.GetAuthorNamesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>());
        _ratings.GetSummariesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, RecipeRatingSummary>());
    }

    private GetRecipesQueryHandler CreateHandler() => new(_recipes, _favorites, _ratings);

    [Fact]
    public async Task Handle_ForwardsEveryFilterDimensionToTheRepository()
    {
        var categoryId = Guid.NewGuid();
        var ingredientIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var query = new GetRecipesQuery(
            Page: 2,
            PageSize: 12,
            Search: "soup",
            CategoryId: categoryId,
            Difficulty: DifficultyLevel.Hard,
            MaxPrepTimeMinutes: 30,
            MaxCookTimeMinutes: 45,
            MinServings: 4,
            IngredientIds: ingredientIds);

        await CreateHandler().Handle(query, CancellationToken.None);

        await _recipes.Received(1).GetPagedAsync(
            2,
            12,
            Arg.Is<RecipeFilter>(f =>
                f.Search == "soup" &&
                f.CategoryId == categoryId &&
                f.Difficulty == DifficultyLevel.Hard &&
                f.MaxPrepTimeMinutes == 30 &&
                f.MaxCookTimeMinutes == 45 &&
                f.MinServings == 4 &&
                f.IngredientIds!.Count == 2),
            Arg.Any<RecipeSortBy>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ForwardsSortSelectionToTheRepository()
    {
        await CreateHandler().Handle(
            new GetRecipesQuery(SortBy: RecipeSortBy.Name, SortDescending: false),
            CancellationToken.None);

        await _recipes.Received(1).GetPagedAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<RecipeFilter>(),
            RecipeSortBy.Name,
            false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DefaultsToNewestFirst()
    {
        await CreateHandler().Handle(new GetRecipesQuery(), CancellationToken.None);

        await _recipes.Received(1).GetPagedAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<RecipeFilter>(),
            RecipeSortBy.DateCreated,
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoFilters_PassesAnEmptyFilter()
    {
        await CreateHandler().Handle(new GetRecipesQuery(), CancellationToken.None);

        await _recipes.Received(1).GetPagedAsync(
            1,
            10,
            Arg.Is<RecipeFilter>(f =>
                f.Search == null &&
                f.CategoryId == null &&
                f.Difficulty == null &&
                f.MaxPrepTimeMinutes == null &&
                f.MaxCookTimeMinutes == null &&
                f.MinServings == null &&
                f.IngredientIds == null),
            Arg.Any<RecipeSortBy>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ClampsPageAndPageSizeBeforeQuerying()
    {
        await CreateHandler().Handle(new GetRecipesQuery(Page: 0, PageSize: 500), CancellationToken.None);

        await _recipes.Received(1).GetPagedAsync(1, 50, Arg.Any<RecipeFilter>(), Arg.Any<RecipeSortBy>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
