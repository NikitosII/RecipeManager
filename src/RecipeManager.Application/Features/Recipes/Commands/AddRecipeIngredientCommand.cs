using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

/// <summary>
/// Attaches an ingredient to a recipe from the free-text recipe editor.
/// The ingredient is looked up by name and created on demand if it does not yet exist.
/// </summary>
public record AddRecipeIngredientCommand(Guid RecipeId, string IngredientName, decimal Quantity, MeasurementUnit Unit) : IRequest<RecipeIngredientDto>;

public class AddRecipeIngredientCommandHandler(IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository)
    : IRequestHandler<AddRecipeIngredientCommand, RecipeIngredientDto>
{
    public async Task<RecipeIngredientDto> Handle(AddRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.IngredientName)) errors.Add("Ingredient name is required.");
        if (request.Quantity <= 0) errors.Add("Quantity must be greater than zero.");
        if (errors.Count > 0) throw new ValidationException(errors);

        var recipe = await recipeRepository.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        var name = request.IngredientName.Trim();
        var ingredient = await ingredientRepository.GetByNameAsync(name, cancellationToken);
        if (ingredient is null)
        {
            ingredient = new Ingredient(name);
            await ingredientRepository.AddAsync(ingredient, cancellationToken);
        }
        recipe.AddIngredient(ingredient.Id, request.Quantity, request.Unit);

        await recipeRepository.SaveChangesAsync(cancellationToken);
        return new RecipeIngredientDto(ingredient.Id, ingredient.Name, request.Quantity, request.Unit);
    }
}
