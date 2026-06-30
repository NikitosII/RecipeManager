using RecipeManager.Domain.Entities;

namespace RecipeManager.Domain.Interfaces;

public interface IIngredientRepository
{
    Task<IReadOnlyList<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default);

    void Update(Ingredient ingredient);

    void Delete(Ingredient ingredient);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
