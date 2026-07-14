using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Features.Recipes;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Collections.Queries;

public record GetCollectionByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<CollectionDetailDto>;

public class GetCollectionByIdQueryHandler(
    ICollectionRepository collectionRepository,
    IRecipeRepository recipeRepository,
    IFavoriteRepository favoriteRepository,
    IRatingRepository ratingRepository) : IRequestHandler<GetCollectionByIdQuery, CollectionDetailDto>
{
    public async Task<CollectionDetailDto> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
    {
        var collection = await collectionRepository.GetByIdWithRecipesAsync(request.Id, cancellationToken)
                         ?? throw new NotFoundException(nameof(Collection), request.Id);

        // Collections are private to their owner.
        collection.EnsureOwnedBy(request.RequestingUserId);

        var recipes = collection.Recipes
            .Select(cr => cr.Recipe!)
            .ToList();

        var recipeIds = recipes.Select(r => r.Id).ToList();

        var authorNames = await recipeRepository.GetAuthorNamesAsync(
            recipes.Select(r => r.UserId).Distinct().ToList(), cancellationToken);

        var favoritedIds = await favoriteRepository.GetFavoritedRecipeIdsAsync(
            request.RequestingUserId, recipeIds, cancellationToken);

        var ratings = await ratingRepository.GetSummariesAsync(
            recipeIds, request.RequestingUserId, cancellationToken);

        var recipeDtos = recipes
            .Select(r => RecipeMapping.ToSummary(
                r, authorNames.GetValueOrDefault(r.UserId, "Unknown"), favoritedIds.Contains(r.Id),
                ratings.GetValueOrDefault(r.Id)))
            .ToList();

        return new CollectionDetailDto(collection.Id, collection.Name, collection.Description, collection.DateCreated, recipeDtos);
    }
}
