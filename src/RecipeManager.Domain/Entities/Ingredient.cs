using RecipeManager.Domain.Common;

namespace RecipeManager.Domain.Entities;

public class Ingredient : BaseEntity
{
    protected Ingredient() { }

    public Ingredient(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = default!;

    public void Update(string name)
    {
        Name = name;
        DateUpdated = DateTime.UtcNow;
    }
}
