using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

/// <summary>
/// Switches a recipe between automatic and manual nutrition. In manual mode the four
/// core macros are required; in automatic mode any stored manual figures are cleared.
/// Returns the resulting per-serving nutrition. Owner-only.
/// </summary>
public record UpdateRecipeNutritionCommand(
    Guid RecipeId,
    NutritionMode Mode,
    decimal? Calories,
    decimal? Protein,
    decimal? Fat,
    decimal? Carbohydrates,
    decimal? Fiber,
    Guid RequestingUserId) : IRequest<NutritionDto>;

public class UpdateRecipeNutritionCommandHandler(IRecipeRepository recipeRepository)
    : IRequestHandler<UpdateRecipeNutritionCommand, NutritionDto>
{
    public async Task<NutritionDto> Handle(UpdateRecipeNutritionCommand request, CancellationToken cancellationToken)
    {
        // Load with details so the returned nutrition reflects a fresh automatic calculation.
        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        recipe.EnsureOwnedBy(request.RequestingUserId);

        if (request.Mode == NutritionMode.Manual)
        {
            var (calories, protein, fat, carbohydrates) = RequireCoreMacros(request);
            recipe.SetManualNutrition(calories, protein, fat, carbohydrates, NonNegativeOrNull(request.Fiber, "Fibre"));
        }
        else
        {
            recipe.UseAutomaticNutrition();
        }

        recipeRepository.Update(recipe);
        await recipeRepository.SaveChangesAsync(cancellationToken);

        return NutritionMapping.Resolve(recipe);
    }

    private static (decimal Calories, decimal Protein, decimal Fat, decimal Carbohydrates) RequireCoreMacros(
        UpdateRecipeNutritionCommand request) =>
    (
        Required(request.Calories, "Calories"),
        Required(request.Protein, "Protein"),
        Required(request.Fat, "Fat"),
        Required(request.Carbohydrates, "Carbohydrates")
    );

    private static decimal Required(decimal? value, string name)
    {
        if (value is not { } v)
            throw new ValidationException([$"{name} is required for manual nutrition."]);
        if (v < 0)
            throw new ValidationException([$"{name} cannot be negative."]);
        return v;
    }

    private static decimal? NonNegativeOrNull(decimal? value, string name)
    {
        if (value is { } v && v < 0)
            throw new ValidationException([$"{name} cannot be negative."]);
        return value;
    }
}
