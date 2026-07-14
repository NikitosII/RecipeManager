using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Features.Recipes;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Favorites.Queries;

public record GetFavoritesQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IRequest<PaginatedResponse<RecipeSummaryDto>>;

public class GetFavoritesQueryHandler(
    IFavoriteRepository favoriteRepository,
    IRecipeRepository recipeRepository,
    IRatingRepository ratingRepository) : IRequestHandler<GetFavoritesQuery, PaginatedResponse<RecipeSummaryDto>>
{
    public async Task<PaginatedResponse<RecipeSummaryDto>> Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var (items, total) = await favoriteRepository.GetFavoritesPagedAsync(
            request.UserId, page, pageSize, cancellationToken);

        var authorNames = await recipeRepository.GetAuthorNamesAsync(
            items.Select(r => r.UserId).Distinct().ToList(), cancellationToken);

        var ratings = await ratingRepository.GetSummariesAsync(
            items.Select(r => r.Id).ToList(), request.UserId, cancellationToken);

        // Everything in this list is, by definition, a favourite of the requesting user.
        var dtos = items.Select(r => RecipeMapping.ToSummary(
            r, authorNames.GetValueOrDefault(r.UserId, "Unknown"), isFavorite: true, ratings.GetValueOrDefault(r.Id)))
            .ToList();

        return new PaginatedResponse<RecipeSummaryDto>(dtos, page, pageSize, total);
    }
}
