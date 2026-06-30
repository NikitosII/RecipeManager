using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record CreateRecipeCommand(
    string Title,
    string? Description,
    DifficultyLevel DifficultyLevel,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Guid CategoryId,
    Guid UserId) : IRequest<Guid>;

public class CreateRecipeCommandHandler(
    IRecipeRepository recipeRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<CreateRecipeCommand, Guid>
{
    public async Task<Guid> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException(["Title is required."]);

        var categoryExists = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (categoryExists is null)
            throw new NotFoundException(nameof(Category), request.CategoryId);

        var recipe = new Recipe(
            request.Title.Trim(),
            request.Description?.Trim(),
            request.DifficultyLevel,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.Servings,
            request.CategoryId,
            request.UserId);

        await recipeRepository.AddAsync(recipe, cancellationToken);
        await recipeRepository.SaveChangesAsync(cancellationToken);

        return recipe.Id;
    }
}
