using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Collections.Commands;

public record DeleteCollectionCommand(Guid Id, Guid RequestingUserId) : IRequest;

public class DeleteCollectionCommandHandler(ICollectionRepository collectionRepository)
    : IRequestHandler<DeleteCollectionCommand>
{
    public async Task Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await collectionRepository.GetByIdAsync(request.Id, cancellationToken)
                         ?? throw new NotFoundException(nameof(Collection), request.Id);

        collection.EnsureOwnedBy(request.RequestingUserId);

        collectionRepository.Delete(collection);
        await collectionRepository.SaveChangesAsync(cancellationToken);
    }
}
