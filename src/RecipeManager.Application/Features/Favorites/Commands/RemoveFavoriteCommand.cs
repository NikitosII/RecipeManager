using MediatR;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Favorites.Commands;

public record RemoveFavoriteCommand(Guid RecipeId, Guid UserId) : IRequest;

public class RemoveFavoriteCommandHandler(IFavoriteRepository favoriteRepository) : IRequestHandler<RemoveFavoriteCommand>
{
    public async Task Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var favorite = await favoriteRepository.GetAsync(request.UserId, request.RecipeId, cancellationToken);
        if (favorite is null)
            return; // Un-favouriting something not favourited is a no-op.

        favoriteRepository.Delete(favorite);
        await favoriteRepository.SaveChangesAsync(cancellationToken);
    }
}
