using MediatR;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record UpdateRecipeCommand(
    Guid Id,
    string Title,
    string? Description,
    DifficultyLevel DifficultyLevel,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings) : IRequest;

public class UpdateRecipeCommandHandler(IRecipeRepository recipeRepository) : IRequestHandler<UpdateRecipeCommand>
{
    public async Task Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException(["Title is required."]);

        var recipe = await recipeRepository.GetByIdAsync(request.Id, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.Id);

        recipe.Update(
            request.Title.Trim(),
            request.Description?.Trim(),
            request.DifficultyLevel,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.Servings);

        recipeRepository.Update(recipe);
        await recipeRepository.SaveChangesAsync(cancellationToken);
    }
}
