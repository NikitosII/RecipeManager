namespace RecipeManager.Domain.Entities;

/// <summary>
/// A user's 1–5 star rating of a recipe.
/// </summary>
public class Rating
{
    public const int MinValue = 1;
    public const int MaxValue = 5;

    protected Rating() { }

    public Rating(Guid userId, Guid recipeId, int value)
    {
        UserId = userId;
        RecipeId = recipeId;
        SetValue(value);
        DateCreated = DateTime.UtcNow;
        DateUpdated = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid RecipeId { get; private set; }
    public int Value { get; private set; }
    public DateTime DateCreated { get; private set; }
    public DateTime DateUpdated { get; private set; }

    public Recipe? Recipe { get; private set; }

    public void UpdateValue(int value)
    {
        SetValue(value);
        DateUpdated = DateTime.UtcNow;
    }

    private void SetValue(int value)
    {
        if (value is < MinValue or > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Rating must be between {MinValue} and {MaxValue}.");

        Value = value;
    }
}
