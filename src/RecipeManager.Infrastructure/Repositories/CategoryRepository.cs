using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

/// <summary>
/// Category repository with an in-memory Dictionary index for O(1) slug lookups
/// </summary>
public class CategoryRepository(RecipeDbContext context) : ICategoryRepository
{
    private Dictionary<string, Category>? _slugIndex;

    private async Task<Dictionary<string, Category>> GetIndexAsync(CancellationToken ct)
    {
        if (_slugIndex is not null) return _slugIndex;

        var all = await context.Categories.ToListAsync(ct);
        _slugIndex = all.ToDictionary(c => c.Slug, StringComparer.OrdinalIgnoreCase);
        return _slugIndex;
    }

    private void InvalidateIndex() => _slugIndex = null;

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Categories
            .Include(c => c.Recipes)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Categories
            .Include(c => c.Recipes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var index = await GetIndexAsync(cancellationToken);
        return index.GetValueOrDefault(slug);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var index = await GetIndexAsync(cancellationToken);
        return index.ContainsKey(slug);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await context.Categories.AddAsync(category, cancellationToken);
        InvalidateIndex();
    }

    public void Update(Category category)
    {
        context.Categories.Update(category);
        InvalidateIndex();
    }

    public void Delete(Category category)
    {
        context.Categories.Remove(category);
        InvalidateIndex();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
