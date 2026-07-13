using MediatR;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record DeleteRecipeCommand(Guid Id, Guid RequestingUserId) : IRequest;

public class DeleteRecipeCommandHandler(IRecipeRepository recipeRepository) : IRequestHandler<DeleteRecipeCommand>
{
    public async Task Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdAsync(request.Id, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.Id);

        recipe.EnsureOwnedBy(request.RequestingUserId);

        recipeRepository.Delete(recipe);
        await recipeRepository.SaveChangesAsync(cancellationToken);
    }
}
