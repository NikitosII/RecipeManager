using RecipeManager.Domain.Entities;

namespace RecipeManager.Domain.Interfaces;

/// <summary>
/// A comment flattened together with its author's display name and avatar.
/// </summary>
public record CommentWithAuthor(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string? AuthorAvatarUrl,
    string Body,
    DateTime DateCreated,
    DateTime DateUpdated);

public interface ICommentRepository
{
    /// <summary>All comments for a recipe with author info, newest last.</summary>
    Task<IReadOnlyList<CommentWithAuthor>> GetForRecipeAsync(Guid recipeId, CancellationToken cancellationToken = default);

    /// <summary>A single comment (tracked) for editing/deletion.</summary>
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The author's display name and avatar, used to build the DTO after a write.</summary>
    Task<(string AuthorName, string? AuthorAvatarUrl)?> GetAuthorAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);

    void Update(Comment comment);

    void Delete(Comment comment);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
