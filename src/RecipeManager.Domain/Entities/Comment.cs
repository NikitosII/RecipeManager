using RecipeManager.Domain.Common;

namespace RecipeManager.Domain.Entities;

/// <summary>
/// A user's free-text comment on a recipe. Owned by its author — only the author may edit or delete it.
/// </summary>
public class Comment : BaseEntity
{
    public const int MaxLength = 2000;

    protected Comment() { }

    public Comment(Guid recipeId, Guid userId, string body)
    {
        RecipeId = recipeId;
        UserId = userId;
        SetBody(body);
    }

    public Guid RecipeId { get; private set; }
    public Guid UserId { get; private set; }
    public string Body { get; private set; } = default!;

    public Recipe? Recipe { get; private set; }

    public void UpdateBody(string body)
    {
        SetBody(body);
        DateUpdated = DateTime.UtcNow;
    }

    private void SetBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Comment body is required.", nameof(body));

        body = body.Trim();
        if (body.Length > MaxLength)
            throw new ArgumentException($"Comment cannot exceed {MaxLength} characters.", nameof(body));

        Body = body;
    }
}
