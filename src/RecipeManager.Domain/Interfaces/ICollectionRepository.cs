using RecipeManager.Domain.Entities;

namespace RecipeManager.Domain.Interfaces;

public interface ICollectionRepository
{
    /// <summary>
    /// The user's collections (membership loaded so recipe counts are available), newest first.
    /// </summary>
    Task<IReadOnlyList<Collection>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// A single collection with its membership loaded — for owner checks and add/remove operations.
    /// </summary>
    Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// A single collection with its member recipes (and their categories) fully loaded — for the detail view.
    /// </summary>
    Task<Collection?> GetByIdWithRecipesAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Collection collection, CancellationToken cancellationToken = default);

    void Update(Collection collection);

    void Delete(Collection collection);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
