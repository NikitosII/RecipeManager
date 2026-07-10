using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Ingredients.Queries;

public record GetIngredientsQuery : IRequest<IReadOnlyList<IngredientDto>>;

public class GetIngredientsQueryHandler(IIngredientRepository ingredientRepository)
    : IRequestHandler<GetIngredientsQuery, IReadOnlyList<IngredientDto>>
{
    public async Task<IReadOnlyList<IngredientDto>> Handle(GetIngredientsQuery request, CancellationToken cancellationToken)
    {
        var ingredients = await ingredientRepository.GetAllAsync(cancellationToken);
        return ingredients.Select(i => new IngredientDto(i.Id, i.Name)).ToList();
    }
}
