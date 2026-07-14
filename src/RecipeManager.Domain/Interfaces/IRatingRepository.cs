using RecipeManager.Domain.Entities;

namespace RecipeManager.Domain.Interfaces;

/// <summary>
/// Aggregated rating information for a single recipe: the mean of all ratings,
/// how many there are, and (optionally) the requesting user's own rating.
/// </summary>
public record RecipeRatingSummary(double Average, int Count, int? UserValue);

public interface IRatingRepository
{
    Task<Rating?> GetAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batched rating aggregates for the given recipes, keyed by recipe id. Recipes
    /// with no ratings are omitted. When <paramref name="requestingUserId"/> is set,
    /// each summary also carries that user's own rating (if any).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, RecipeRatingSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> recipeIds, Guid? requestingUserId, CancellationToken cancellationToken = default);

    Task AddAsync(Rating rating, CancellationToken cancellationToken = default);

    void Delete(Rating rating);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
