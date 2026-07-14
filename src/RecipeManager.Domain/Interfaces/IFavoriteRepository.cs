using RecipeManager.Domain.Entities;

namespace RecipeManager.Domain.Interfaces;

public interface IFavoriteRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);

    Task<Favorite?> GetAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The current user's favourite recipes, newest favourite first, paged.
    /// </summary>
    Task<(IReadOnlyList<Recipe> Items, int TotalCount)> GetFavoritesPagedAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Of the supplied recipe ids, the subset the user has favourited — used to
    /// stamp the <c>IsFavorite</c> flag onto recipe listings without N+1 queries.
    /// </summary>
    Task<IReadOnlySet<Guid>> GetFavoritedRecipeIdsAsync(
        Guid userId, IReadOnlyCollection<Guid> recipeIds, CancellationToken cancellationToken = default);

    Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);

    void Delete(Favorite favorite);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
