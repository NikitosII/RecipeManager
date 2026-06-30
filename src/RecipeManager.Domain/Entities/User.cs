using RecipeManager.Domain.Common;

namespace RecipeManager.Domain.Entities;

public class User : BaseEntity
{
    private readonly List<Recipe> _recipes = [];

    protected User() { }

    public User(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    public IReadOnlyList<Recipe> Recipes => _recipes.AsReadOnly();

    public void Update(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        DateUpdated = DateTime.UtcNow;
    }
}
