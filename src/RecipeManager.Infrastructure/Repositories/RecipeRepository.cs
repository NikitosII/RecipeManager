using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

public class RecipeRepository(RecipeDbContext context) : IRecipeRepository
{
    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, RecipeFilter filter, RecipeSortBy sortBy = RecipeSortBy.DateCreated, bool sortDescending = true, CancellationToken cancellationToken = default)
    {
        var query = context.Recipes
            .Include(r => r.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search;
            query = query.Where(r =>
                r.Title.Contains(term) ||
                (r.Description != null && r.Description.Contains(term)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == filter.CategoryId.Value);

        if (filter.Difficulty.HasValue)
            query = query.Where(r => r.DifficultyLevel == filter.Difficulty.Value);

        if (filter.MaxPrepTimeMinutes.HasValue)
            query = query.Where(r => r.PrepTimeMinutes <= filter.MaxPrepTimeMinutes.Value);

        if (filter.MaxCookTimeMinutes.HasValue)
            query = query.Where(r => r.CookTimeMinutes <= filter.MaxCookTimeMinutes.Value);

        if (filter.MinServings.HasValue)
            query = query.Where(r => r.Servings >= filter.MinServings.Value);

        // Recipes that contain *all* of the requested ingredients: the count of
        // matched ingredients on the recipe must equal the number requested.
        if (filter.IngredientIds is { Count: > 0 })
        {
            var ingredientIds = filter.IngredientIds.ToList();
            var required = ingredientIds.Count;
            query = query.Where(r =>
                r.RecipeIngredients.Count(ri => ingredientIds.Contains(ri.IngredientId)) == required);
        }

        var total = await query.CountAsync(cancellationToken);

        var ordered = sortBy switch
        {
            RecipeSortBy.Name => sortDescending
                ? query.OrderByDescending(r => r.Title)
                : query.OrderBy(r => r.Title),
            RecipeSortBy.Rating => sortDescending
                ? query.OrderByDescending(r => context.Ratings.Where(rt => rt.RecipeId == r.Id).Average(rt => (double?)rt.Value) ?? 0)
                : query.OrderBy(r => context.Ratings.Where(rt => rt.RecipeId == r.Id).Average(rt => (double?)rt.Value) ?? 0),
            _ => sortDescending
                ? query.OrderByDescending(r => r.DateCreated)
                : query.OrderBy(r => r.DateCreated),
        };

        var items = await ordered
            .ThenByDescending(r => r.DateCreated)
            .ThenBy(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Recipes.FindAsync([id], cancellationToken);

    public async Task<Recipe?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Recipes
            .Include(r => r.Category)
            .Include(r => r.Steps)
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, string>> GetAuthorNamesAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, string>();

        return await context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), cancellationToken);
    }

    public async Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default)
        => await context.Recipes.AddAsync(recipe, cancellationToken);

    public void Update(Recipe recipe)
        => context.Recipes.Update(recipe);

    public void Delete(Recipe recipe)
        => context.Recipes.Remove(recipe);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
