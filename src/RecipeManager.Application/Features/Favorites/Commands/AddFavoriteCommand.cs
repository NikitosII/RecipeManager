using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Favorites.Commands;

public record AddFavoriteCommand(Guid RecipeId, Guid UserId) : IRequest;

public class AddFavoriteCommandHandler(
    IFavoriteRepository favoriteRepository,
    IRecipeRepository recipeRepository) : IRequestHandler<AddFavoriteCommand>
{
    public async Task Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        _ = await recipeRepository.GetByIdAsync(request.RecipeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Recipe), request.RecipeId);

        // Favouriting is idempotent — re-adding an existing favourite is a no-op.
        if (await favoriteRepository.ExistsAsync(request.UserId, request.RecipeId, cancellationToken))
            return;

        await favoriteRepository.AddAsync(new Favorite(request.UserId, request.RecipeId), cancellationToken);
        await favoriteRepository.SaveChangesAsync(cancellationToken);
    }
}
