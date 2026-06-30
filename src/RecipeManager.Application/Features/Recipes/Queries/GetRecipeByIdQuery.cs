using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Queries;

public record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeDetailDto>;

public class GetRecipeByIdQueryHandler(IRecipeRepository recipeRepository) : IRequestHandler<GetRecipeByIdQuery, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.Id);

        return new RecipeDetailDto(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.DifficultyLevel,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.Servings,
            recipe.ImageUrl,
            recipe.CategoryId,
            recipe.Category?.Name ?? string.Empty,
            recipe.UserId,
            recipe.Steps.Select(s => new RecipeStepDto(s.Id, s.StepNumber, s.Description)).ToList(),
            recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto(
                ri.IngredientId,
                ri.Ingredient?.Name ?? string.Empty,
                ri.Quantity,
                ri.Unit)).ToList(),
            recipe.DateCreated,
            recipe.DateUpdated);
    }
}
