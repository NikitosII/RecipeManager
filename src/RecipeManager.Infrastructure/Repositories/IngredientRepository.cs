using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

public class IngredientRepository(RecipeDbContext context) : IIngredientRepository
{
    public async Task<IReadOnlyList<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Ingredients.OrderBy(i => i.Name).ToListAsync(cancellationToken);

    public async Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Ingredients.FindAsync([id], cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => await context.Ingredients.AnyAsync(i => i.Name == name, cancellationToken);

    public async Task AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
        => await context.Ingredients.AddAsync(ingredient, cancellationToken);

    public void Update(Ingredient ingredient)
        => context.Ingredients.Update(ingredient);

    public void Delete(Ingredient ingredient)
        => context.Ingredients.Remove(ingredient);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
