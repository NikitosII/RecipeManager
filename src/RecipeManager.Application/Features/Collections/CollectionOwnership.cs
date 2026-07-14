using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Application.Features.Collections;

internal static class CollectionOwnership
{
    /// <summary>
    /// Guards a collection mutation: only the user who owns the collection may change it.
    /// </summary>
    public static void EnsureOwnedBy(this Collection collection, Guid userId)
    {
        if (collection.UserId != userId)
            throw new ForbiddenException("You can only modify collections that you own.");
    }
}
