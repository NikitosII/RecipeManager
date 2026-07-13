using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Application.Features.Recipes;

internal static class RecipeOwnership
{
    /// <summary>
    /// Guards a recipe mutation: only the user who created the recipe may change it.
    /// </summary>
    public static void EnsureOwnedBy(this Recipe recipe, Guid userId)
    {
        if (recipe.UserId != userId)
            throw new ForbiddenException("You can only modify recipes that you created.");
    }
}
