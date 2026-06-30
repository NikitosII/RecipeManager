using RecipeManager.Domain.Entities;

namespace RecipeManager.Domain.Interfaces;

public interface IRecipeRepository
{
    Task<(IReadOnlyList<Recipe> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm,
        Guid? categoryId,
        CancellationToken cancellationToken = default);

    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Recipe?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default);

    void Update(Recipe recipe);

    void Delete(Recipe recipe);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
