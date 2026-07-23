using Microsoft.EntityFrameworkCore;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure.Repositories;

public class CommentRepository(RecipeDbContext context) : ICommentRepository
{
    public async Task<IReadOnlyList<CommentWithAuthor>> GetForRecipeAsync(Guid recipeId, CancellationToken cancellationToken = default)
        => await context.Comments
            .Where(c => c.RecipeId == recipeId)
            .OrderBy(c => c.DateCreated)
            // Join to the identity store so the query handler stays free of user concerns.
            .Join(context.Users,
                c => c.UserId,
                u => u.Id,
                (c, u) => new CommentWithAuthor(
                    c.Id,
                    c.UserId,
                    $"{u.FirstName} {u.LastName}".Trim(),
                    u.AvatarUrl,
                    c.Body,
                    c.DateCreated,
                    c.DateUpdated))
            .ToListAsync(cancellationToken);

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(string AuthorName, string? AuthorAvatarUrl)?> GetAuthorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var author = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName, u.AvatarUrl })
            .FirstOrDefaultAsync(cancellationToken);

        return author is null
            ? null
            : ($"{author.FirstName} {author.LastName}".Trim(), author.AvatarUrl);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
        => await context.Comments.AddAsync(comment, cancellationToken);

    public void Update(Comment comment)
        => context.Comments.Update(comment);

    public void Delete(Comment comment)
        => context.Comments.Remove(comment);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
