using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

public class RecipeRepository(RecipeDbContext context) : IRecipeRepository
{
    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Recipes
            .Include(r => r.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(r =>
                r.Title.Contains(searchTerm) ||
                (r.Description != null && r.Description.Contains(searchTerm)));

        if (categoryId.HasValue)
            query = query.Where(r => r.CategoryId == categoryId.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.DateCreated)
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

    public async Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default)
        => await context.Recipes.AddAsync(recipe, cancellationToken);

    public void Update(Recipe recipe)
        => context.Recipes.Update(recipe);

    public void Delete(Recipe recipe)
        => context.Recipes.Remove(recipe);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
