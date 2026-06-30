using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record InsertRecipeStepCommand(Guid RecipeId, int AfterStepNumber, string Description) : IRequest<RecipeStepDto>;

public class InsertRecipeStepCommandHandler(IRecipeRepository recipeRepository) : IRequestHandler<InsertRecipeStepCommand, RecipeStepDto>
{
    public async Task<RecipeStepDto> Handle(InsertRecipeStepCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ValidationException(["Step description is required."]);

        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        // Domain method uses LinkedList traversal internally
        var step = recipe.InsertStepAfter(request.AfterStepNumber, request.Description.Trim());

        recipeRepository.Update(recipe);
        await recipeRepository.SaveChangesAsync(cancellationToken);

        return new RecipeStepDto(step.Id, step.StepNumber, step.Description);
    }
}
