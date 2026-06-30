using RecipeManager.Domain.Entities;

namespace RecipeManager.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// O(1) lookup backed by an in-memory Dictionary index keyed by slug.
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    void Update(Category category);

    void Delete(Category category);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
