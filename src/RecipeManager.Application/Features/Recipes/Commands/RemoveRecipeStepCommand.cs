using MediatR;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record RemoveRecipeStepCommand(Guid RecipeId, int StepNumber, Guid RequestingUserId) : IRequest;

public class RemoveRecipeStepCommandHandler(IRecipeRepository recipeRepository) : IRequestHandler<RemoveRecipeStepCommand>
{
    public async Task Handle(RemoveRecipeStepCommand request, CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        recipe.EnsureOwnedBy(request.RequestingUserId);

        recipe.RemoveStep(request.StepNumber);
        await recipeRepository.SaveChangesAsync(cancellationToken);
    }
}
