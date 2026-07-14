using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Collections.Commands;

public record RemoveRecipeFromCollectionCommand(Guid CollectionId, Guid RecipeId, Guid RequestingUserId) : IRequest;

public class RemoveRecipeFromCollectionCommandHandler(ICollectionRepository collectionRepository)
    : IRequestHandler<RemoveRecipeFromCollectionCommand>
{
    public async Task Handle(RemoveRecipeFromCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken)
                         ?? throw new NotFoundException(nameof(Collection), request.CollectionId);

        collection.EnsureOwnedBy(request.RequestingUserId);

        collection.RemoveRecipe(request.RecipeId);
        await collectionRepository.SaveChangesAsync(cancellationToken);
    }
}
