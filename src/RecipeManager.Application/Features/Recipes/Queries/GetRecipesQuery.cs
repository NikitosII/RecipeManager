using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Queries;

public record GetRecipesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? CategoryId = null,
    DifficultyLevel? Difficulty = null,
    int? MaxPrepTimeMinutes = null,
    int? MaxCookTimeMinutes = null,
    int? MinServings = null,
    IReadOnlyCollection<Guid>? IngredientIds = null,
    RecipeSortBy SortBy = RecipeSortBy.DateCreated,
    bool SortDescending = true,
    Guid? RequestingUserId = null) : IRequest<PaginatedResponse<RecipeSummaryDto>>;

public class GetRecipesQueryHandler(
    IRecipeRepository recipeRepository,
    IFavoriteRepository favoriteRepository,
    IRatingRepository ratingRepository) : IRequestHandler<GetRecipesQuery, PaginatedResponse<RecipeSummaryDto>>
{
    public async Task<PaginatedResponse<RecipeSummaryDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var filter = new RecipeFilter(
            request.Search,
            request.CategoryId,
            request.Difficulty,
            request.MaxPrepTimeMinutes,
            request.MaxCookTimeMinutes,
            request.MinServings,
            request.IngredientIds);

        var (items, total) = await recipeRepository.GetPagedAsync(
            page, pageSize, filter, request.SortBy, request.SortDescending, cancellationToken);

        var recipeIds = items.Select(r => r.Id).ToList();

        var authorNames = await recipeRepository.GetAuthorNamesAsync(
            items.Select(r => r.UserId).Distinct().ToList(), cancellationToken);

        var favoritedIds = request.RequestingUserId is { } userId
            ? await favoriteRepository.GetFavoritedRecipeIdsAsync(userId, recipeIds, cancellationToken)
            : (IReadOnlySet<Guid>)new HashSet<Guid>();

        var ratings = await ratingRepository.GetSummariesAsync(
            recipeIds, request.RequestingUserId, cancellationToken);

        var dtos = items.Select(r => RecipeMapping.ToSummary(
            r,
            authorNames.GetValueOrDefault(r.UserId, "Unknown"),
            favoritedIds.Contains(r.Id),
            ratings.GetValueOrDefault(r.Id))).ToList();

        return new PaginatedResponse<RecipeSummaryDto>(dtos, page, pageSize, total);
    }
}
