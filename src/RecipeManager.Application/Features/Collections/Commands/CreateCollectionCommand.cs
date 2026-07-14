using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Collections.Commands;

public record CreateCollectionCommand(string Name, string? Description, Guid UserId) : IRequest<Guid>;

public class CreateCollectionCommandHandler(ICollectionRepository collectionRepository)
    : IRequestHandler<CreateCollectionCommand, Guid>
{
    public async Task<Guid> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException(["Name is required."]);

        var collection = new Collection(request.Name.Trim(), request.Description?.Trim(), request.UserId);
        await collectionRepository.AddAsync(collection, cancellationToken);
        await collectionRepository.SaveChangesAsync(cancellationToken);

        return collection.Id;
    }
}
