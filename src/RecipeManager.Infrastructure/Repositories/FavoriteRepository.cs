using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

public class FavoriteRepository(RecipeDbContext context) : IFavoriteRepository
{
    public Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        => context.Favorites.AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);

    public async Task<Favorite?> GetAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        => await context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);

    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)> GetFavoritesPagedAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Page over the favourites (ordered by when they were favourited), then load the
        // recipes with their category. Done in two steps because EF Core does not allow
        // Include() after a Select() projection.
        var favoritesQuery = context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.DateCreated);

        var total = await favoritesQuery.CountAsync(cancellationToken);

        var orderedIds = await favoritesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.RecipeId)
            .ToListAsync(cancellationToken);

        var recipes = await context.Recipes
            .Include(r => r.Category)
            .Where(r => orderedIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        // Preserve the favourite ordering lost by the id-based reload.
        var items = orderedIds
            .Select(id => recipes.First(r => r.Id == id))
            .ToList();

        return (items, total);
    }

    public Task<int> CountAsync(Guid userId, CancellationToken cancellationToken = default)
        => context.Favorites.CountAsync(f => f.UserId == userId, cancellationToken);

    public async Task<IReadOnlySet<Guid>> GetFavoritedRecipeIdsAsync(
        Guid userId, IReadOnlyCollection<Guid> recipeIds, CancellationToken cancellationToken = default)
    {
        if (recipeIds.Count == 0)
            return new HashSet<Guid>();

        var ids = await context.Favorites
            .Where(f => f.UserId == userId && recipeIds.Contains(f.RecipeId))
            .Select(f => f.RecipeId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
        => await context.Favorites.AddAsync(favorite, cancellationToken);

    public void Delete(Favorite favorite)
        => context.Favorites.Remove(favorite);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
