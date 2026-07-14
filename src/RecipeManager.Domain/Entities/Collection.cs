using RecipeManager.Domain.Common;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Domain.Entities;

/// <summary>
/// A user-owned, named grouping of recipes (like a playlist). A recipe may
/// belong to many collections; membership is held in <see cref="CollectionRecipe"/>.
/// </summary>
public class Collection : BaseEntity
{
    private readonly List<CollectionRecipe> _recipes = [];

    protected Collection() { }

    public Collection(string name, string? description, Guid userId)
    {
        Name = name;
        Description = description;
        UserId = userId;
    }

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    public Guid UserId { get; private set; }

    // EF binds this to the backing field; ordering is done at query level.
    public IReadOnlyList<CollectionRecipe> Recipes => _recipes;

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        DateUpdated = DateTime.UtcNow;
    }

    public void AddRecipe(Guid recipeId)
    {
        if (_recipes.Any(r => r.RecipeId == recipeId))
            throw new ConflictException("This recipe is already in the collection.");

        _recipes.Add(new CollectionRecipe(Id, recipeId));
        DateUpdated = DateTime.UtcNow;
    }

    public void RemoveRecipe(Guid recipeId)
    {
        var membership = _recipes.FirstOrDefault(r => r.RecipeId == recipeId)
                         ?? throw new NotFoundException(nameof(CollectionRecipe), recipeId);

        _recipes.Remove(membership);
        DateUpdated = DateTime.UtcNow;
    }
}
