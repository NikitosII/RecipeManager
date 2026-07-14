using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Collections.Queries;

public record GetCollectionsQuery(Guid UserId) : IRequest<IReadOnlyList<CollectionSummaryDto>>;

public class GetCollectionsQueryHandler(ICollectionRepository collectionRepository)
    : IRequestHandler<GetCollectionsQuery, IReadOnlyList<CollectionSummaryDto>>
{
    public async Task<IReadOnlyList<CollectionSummaryDto>> Handle(GetCollectionsQuery request, CancellationToken cancellationToken)
    {
        var collections = await collectionRepository.GetByUserAsync(request.UserId, cancellationToken);

        return collections
            .Select(c => new CollectionSummaryDto(c.Id, c.Name, c.Description, c.Recipes.Count, c.DateCreated))
            .ToList();
    }
}
