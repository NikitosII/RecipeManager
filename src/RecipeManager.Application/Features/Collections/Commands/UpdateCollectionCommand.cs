using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Collections.Commands;

public record UpdateCollectionCommand(Guid Id, string Name, string? Description, Guid RequestingUserId) : IRequest;

public class UpdateCollectionCommandHandler(ICollectionRepository collectionRepository)
    : IRequestHandler<UpdateCollectionCommand>
{
    public async Task Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException(["Name is required."]);

        var collection = await collectionRepository.GetByIdAsync(request.Id, cancellationToken)
                         ?? throw new NotFoundException(nameof(Collection), request.Id);

        collection.EnsureOwnedBy(request.RequestingUserId);

        collection.Update(request.Name.Trim(), request.Description?.Trim());
        collectionRepository.Update(collection);
        await collectionRepository.SaveChangesAsync(cancellationToken);
    }
}
