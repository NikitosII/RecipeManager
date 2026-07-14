using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Collections.Commands;

public record AddRecipeToCollectionCommand(Guid CollectionId, Guid RecipeId, Guid RequestingUserId) : IRequest;

public class AddRecipeToCollectionCommandHandler(
    ICollectionRepository collectionRepository,
    IRecipeRepository recipeRepository) : IRequestHandler<AddRecipeToCollectionCommand>
{
    public async Task Handle(AddRecipeToCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken)
                         ?? throw new NotFoundException(nameof(Collection), request.CollectionId);

        collection.EnsureOwnedBy(request.RequestingUserId);

        _ = await recipeRepository.GetByIdAsync(request.RecipeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Recipe), request.RecipeId);

        collection.AddRecipe(request.RecipeId);
        await collectionRepository.SaveChangesAsync(cancellationToken);
    }
}
