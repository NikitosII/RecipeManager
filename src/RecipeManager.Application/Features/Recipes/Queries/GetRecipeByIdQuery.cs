using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Queries;

public record GetRecipeByIdQuery(Guid Id, Guid? RequestingUserId = null) : IRequest<RecipeDetailDto>;

public class GetRecipeByIdQueryHandler(
    IRecipeRepository recipeRepository,
    IFavoriteRepository favoriteRepository,
    IRatingRepository ratingRepository) : IRequestHandler<GetRecipeByIdQuery, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.Id);

        var authorNames = await recipeRepository.GetAuthorNamesAsync([recipe.UserId], cancellationToken);

        var isFavorite = request.RequestingUserId is { } userId
            && await favoriteRepository.ExistsAsync(userId, recipe.Id, cancellationToken);

        var ratings = await ratingRepository.GetSummariesAsync(
            [recipe.Id], request.RequestingUserId, cancellationToken);
        var rating = ratings.GetValueOrDefault(recipe.Id);

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
            authorNames.GetValueOrDefault(recipe.UserId, "Unknown"),
            isFavorite,
            rating?.Average ?? 0,
            rating?.Count ?? 0,
            rating?.UserValue,
            recipe.Steps.Select(s => new RecipeStepDto(s.Id, s.StepNumber, s.Description)).ToList(),
            recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto(
                ri.IngredientId,
                ri.Ingredient?.Name ?? string.Empty,
                ri.Quantity,
                ri.Unit)).ToList(),
            NutritionMapping.Resolve(recipe),
            recipe.DateCreated,
            recipe.DateUpdated);
    }
}
