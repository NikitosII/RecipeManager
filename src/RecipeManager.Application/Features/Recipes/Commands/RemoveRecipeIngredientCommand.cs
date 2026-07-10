using MediatR;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record RemoveRecipeIngredientCommand(Guid RecipeId, Guid IngredientId) : IRequest;

public class RemoveRecipeIngredientCommandHandler(IRecipeRepository recipeRepository)
    : IRequestHandler<RemoveRecipeIngredientCommand>
{
    public async Task Handle(RemoveRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        recipe.RemoveIngredient(request.IngredientId);
        await recipeRepository.SaveChangesAsync(cancellationToken);
    }
}
