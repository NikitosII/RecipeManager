using RecipeManager.Domain.Common;

namespace RecipeManager.Domain.Entities;

public class Category : BaseEntity
{
    private readonly List<Recipe> _recipes = [];

    protected Category() { }

    public Category(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public string Name { get; private set; } = default!;

    // URL-safe identifier; used as the key in the category Dictionary index.
    public string Slug { get; private set; } = default!;

    public IReadOnlyList<Recipe> Recipes => _recipes.AsReadOnly();

    public void Update(string name, string slug)
    {
        Name = name;
        Slug = slug;
        DateUpdated = DateTime.UtcNow;
    }
}
