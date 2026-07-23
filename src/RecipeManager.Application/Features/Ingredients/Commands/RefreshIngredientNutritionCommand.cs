using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Ingredients.Commands;

/// <summary>
/// Re-looks-up an ingredient's per-100g macros from the nutrition source and caches
/// them. Used to backfill ingredients created before nutrition existed, or to refresh
/// stale data. Returns the current cached state.
/// </summary>
public record RefreshIngredientNutritionCommand(Guid IngredientId) : IRequest<IngredientNutritionDto>;

public class RefreshIngredientNutritionCommandHandler(IIngredientRepository ingredientRepository, INutritionProvider nutritionProvider)
    : IRequestHandler<RefreshIngredientNutritionCommand, IngredientNutritionDto>
{
    public async Task<IngredientNutritionDto> Handle(RefreshIngredientNutritionCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await ingredientRepository.GetByIdAsync(request.IngredientId, cancellationToken)
                         ?? throw new NotFoundException(nameof(Domain.Entities.Ingredient), request.IngredientId);

        // interactive "fetch now" action
        NutritionLookup? facts;
        try
        {
            facts = await nutritionProvider.LookupAsync(ingredient.Name, cancellationToken);
        }
        catch (NutritionProviderUnavailableException)
        {
            facts = null;
        }

        if (facts is not null)
        {
            ingredient.SetNutritionFacts(
                facts.CaloriesPer100g, facts.ProteinPer100g, facts.FatPer100g,
                facts.CarbsPer100g, facts.FiberPer100g);

            ingredientRepository.Update(ingredient);
            await ingredientRepository.SaveChangesAsync(cancellationToken);
        }

        return new IngredientNutritionDto(
            ingredient.Id,
            ingredient.Name,
            ingredient.HasNutrition,
            ingredient.CaloriesPer100g,
            ingredient.ProteinPer100g,
            ingredient.FatPer100g,
            ingredient.CarbsPer100g,
            ingredient.FiberPer100g);
    }
}
