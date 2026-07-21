using MediatR;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Ingredients.Commands;

public record CreateIngredientCommand(string Name) : IRequest<Guid>;

public class CreateIngredientCommandHandler(IIngredientRepository ingredientRepository, INutritionProvider nutritionProvider)
    : IRequestHandler<CreateIngredientCommand, Guid>
{
    public async Task<Guid> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException(["Ingredient name is required."]);

        var name = request.Name.Trim();

        var existing = await ingredientRepository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"An ingredient named '{name}' already exists.");

        var ingredient = new Ingredient(name);
        await IngredientEnrichment.EnrichAsync(ingredient, nutritionProvider, cancellationToken);

        await ingredientRepository.AddAsync(ingredient, cancellationToken);
        await ingredientRepository.SaveChangesAsync(cancellationToken);

        return ingredient.Id;
    }
}
