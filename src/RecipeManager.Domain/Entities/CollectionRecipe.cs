namespace RecipeManager.Domain.Entities;

/// <summary>
/// Membership of a recipe in a collection. Pure join entity with a composite
/// (CollectionId, RecipeId) key.
/// </summary>
public class CollectionRecipe
{
    protected CollectionRecipe() { }

    internal CollectionRecipe(Guid collectionId, Guid recipeId)
    {
        CollectionId = collectionId;
        RecipeId = recipeId;
        DateCreated = DateTime.UtcNow;
    }

    public Guid CollectionId { get; private set; }
    public Guid RecipeId { get; private set; }
    public DateTime DateCreated { get; private set; }

    public Collection? Collection { get; private set; }
    public Recipe? Recipe { get; private set; }
}
