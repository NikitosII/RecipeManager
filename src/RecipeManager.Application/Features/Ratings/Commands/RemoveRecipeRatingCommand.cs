using MediatR;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Ratings.Commands;

public record RemoveRecipeRatingCommand(Guid RecipeId, Guid UserId) : IRequest;

public class RemoveRecipeRatingCommandHandler(IRatingRepository ratingRepository)
    : IRequestHandler<RemoveRecipeRatingCommand>
{
    public async Task Handle(RemoveRecipeRatingCommand request, CancellationToken cancellationToken)
    {
        var rating = await ratingRepository.GetAsync(request.UserId, request.RecipeId, cancellationToken);
        if (rating is null)
            return; // Removing a rating that was never given is a no-op.

        ratingRepository.Delete(rating);
        await ratingRepository.SaveChangesAsync(cancellationToken);
    }
}
