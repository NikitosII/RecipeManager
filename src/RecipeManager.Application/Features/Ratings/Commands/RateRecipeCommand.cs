using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Ratings.Commands;

public record RateRecipeCommand(Guid RecipeId, Guid UserId, int Value) : IRequest;

public class RateRecipeCommandHandler(
    IRatingRepository ratingRepository,
    IRecipeRepository recipeRepository) : IRequestHandler<RateRecipeCommand>
{
    public async Task Handle(RateRecipeCommand request, CancellationToken cancellationToken)
    {
        if (request.Value is < Rating.MinValue or > Rating.MaxValue)
            throw new ValidationException([$"Rating must be between {Rating.MinValue} and {Rating.MaxValue}."]);

        _ = await recipeRepository.GetByIdAsync(request.RecipeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Recipe), request.RecipeId);

        var existing = await ratingRepository.GetAsync(request.UserId, request.RecipeId, cancellationToken);
        if (existing is null)
            await ratingRepository.AddAsync(new Rating(request.UserId, request.RecipeId, request.Value), cancellationToken);
        else
            existing.UpdateValue(request.Value);

        await ratingRepository.SaveChangesAsync(cancellationToken);
    }
}
