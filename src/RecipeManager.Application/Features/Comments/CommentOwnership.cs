using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Application.Features.Comments;

internal static class CommentOwnership
{
    /// <summary>
    /// Guards a comment mutation: only the author may edit or delete their comment.
    /// </summary>
    public static void EnsureOwnedBy(this Comment comment, Guid userId)
    {
        if (comment.UserId != userId)
            throw new ForbiddenException("You can only modify your own comments.");
    }
}
