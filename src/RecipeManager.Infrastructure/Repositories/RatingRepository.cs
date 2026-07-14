using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

public class RatingRepository(RecipeDbContext context) : IRatingRepository
{
    public async Task<Rating?> GetAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        => await context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, RecipeRatingSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> recipeIds, Guid? requestingUserId, CancellationToken cancellationToken = default)
    {
        if (recipeIds.Count == 0)
            return new Dictionary<Guid, RecipeRatingSummary>();

        var aggregates = await context.Ratings
            .Where(r => recipeIds.Contains(r.RecipeId))
            .GroupBy(r => r.RecipeId)
            .Select(g => new { RecipeId = g.Key, Average = g.Average(x => (double)x.Value), Count = g.Count() })
            .ToListAsync(cancellationToken);

        var ownRatings = requestingUserId is { } userId
            ? await context.Ratings
                .Where(r => r.UserId == userId && recipeIds.Contains(r.RecipeId))
                .Select(r => new { r.RecipeId, r.Value })
                .ToDictionaryAsync(r => r.RecipeId, r => r.Value, cancellationToken)
            : new Dictionary<Guid, int>();

        return aggregates.ToDictionary(
            a => a.RecipeId,
            a => new RecipeRatingSummary(a.Average, a.Count, ownRatings.TryGetValue(a.RecipeId, out var v) ? v : null));
    }

    public async Task AddAsync(Rating rating, CancellationToken cancellationToken = default)
        => await context.Ratings.AddAsync(rating, cancellationToken);

    public void Delete(Rating rating)
        => context.Ratings.Remove(rating);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
