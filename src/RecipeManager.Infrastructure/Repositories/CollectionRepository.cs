using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

public class CollectionRepository(RecipeDbContext context) : ICollectionRepository
{
    public async Task<IReadOnlyList<Collection>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.Collections
            .Where(c => c.UserId == userId)
            .Include(c => c.Recipes)
            .OrderByDescending(c => c.DateCreated)
            .ToListAsync(cancellationToken);

    public async Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Collections
            .Include(c => c.Recipes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Collection?> GetByIdWithRecipesAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Collections
            .Include(c => c.Recipes.OrderByDescending(cr => cr.DateCreated))
                .ThenInclude(cr => cr.Recipe!)
                    .ThenInclude(r => r.Category)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Collection collection, CancellationToken cancellationToken = default)
        => await context.Collections.AddAsync(collection, cancellationToken);

    public void Update(Collection collection)
        => context.Collections.Update(collection);

    public void Delete(Collection collection)
        => context.Collections.Remove(collection);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
