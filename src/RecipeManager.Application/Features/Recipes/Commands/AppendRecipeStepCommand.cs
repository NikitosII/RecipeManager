using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record AppendRecipeStepCommand(Guid RecipeId, string Description) : IRequest<RecipeStepDto>;

public class AppendRecipeStepCommandHandler(IRecipeRepository recipeRepository) : IRequestHandler<AppendRecipeStepCommand, RecipeStepDto>
{
    public async Task<RecipeStepDto> Handle(AppendRecipeStepCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ValidationException(["Step description is required."]);

        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        var step = recipe.AppendStep(request.Description.Trim());

        recipeRepository.Update(recipe);
        await recipeRepository.SaveChangesAsync(cancellationToken);

        return new RecipeStepDto(step.Id, step.StepNumber, step.Description);
    }
}
