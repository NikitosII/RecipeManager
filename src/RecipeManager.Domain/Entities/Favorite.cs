namespace RecipeManager.Domain.Entities;

/// <summary>
/// A user's favourite ("liked") recipe. Pure join entity with a composite
/// (UserId, RecipeId) key — a user can favourite a given recipe at most once.
/// </summary>
public class Favorite
{
    protected Favorite() { }

    public Favorite(Guid userId, Guid recipeId)
    {
        UserId = userId;
        RecipeId = recipeId;
        DateCreated = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid RecipeId { get; private set; }
    public DateTime DateCreated { get; private set; }

    public Recipe? Recipe { get; private set; }
}
