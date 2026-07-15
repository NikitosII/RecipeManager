using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;

namespace RecipeManager.Domain.Interfaces;

/// <summary>
/// Optional criteria for a recipe listing.
/// </summary>
public record RecipeFilter(
    string? Search = null,
    Guid? CategoryId = null,
    DifficultyLevel? Difficulty = null,
    int? MaxPrepTimeMinutes = null,
    int? MaxCookTimeMinutes = null,
    int? MinServings = null,
    IReadOnlyCollection<Guid>? IngredientIds = null);

public interface IRecipeRepository
{
    Task<(IReadOnlyList<Recipe> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        RecipeFilter filter,
        RecipeSortBy sortBy = RecipeSortBy.DateCreated,
        bool sortDescending = true,
        CancellationToken cancellationToken = default);

    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Recipe?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, string>> GetAuthorNamesAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);

    Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default);

    void Update(Recipe recipe);

    void Delete(Recipe recipe);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
