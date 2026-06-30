using MediatR;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record RemoveRecipeStepCommand(Guid RecipeId, int StepNumber) : IRequest;

public class RemoveRecipeStepCommandHandler(IRecipeRepository recipeRepository) : IRequestHandler<RemoveRecipeStepCommand>
{
    public async Task Handle(RemoveRecipeStepCommand request, CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        recipe.RemoveStep(request.StepNumber);

        recipeRepository.Update(recipe);
        await recipeRepository.SaveChangesAsync(cancellationToken);
    }
}
