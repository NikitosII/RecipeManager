using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Queries;

public record GetRecipesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? CategoryId = null) : IRequest<PaginatedResponse<RecipeSummaryDto>>;

public class GetRecipesQueryHandler(IRecipeRepository recipeRepository) : IRequestHandler<GetRecipesQuery, PaginatedResponse<RecipeSummaryDto>>
{
    public async Task<PaginatedResponse<RecipeSummaryDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var (items, total) = await recipeRepository.GetPagedAsync(
            page, pageSize, request.Search, request.CategoryId, cancellationToken);

        var dtos = items.Select(r => new RecipeSummaryDto(
            r.Id,
            r.Title,
            r.Description,
            r.DifficultyLevel,
            r.PrepTimeMinutes,
            r.CookTimeMinutes,
            r.Servings,
            r.ImageUrl,
            r.Category?.Name ?? string.Empty,
            r.DateCreated)).ToList();

        return new PaginatedResponse<RecipeSummaryDto>(dtos, page, pageSize, total);
    }
}
