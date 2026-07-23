using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Application.Features.Comments;

internal static class CommentValidation
{
    /// <summary>
    /// Validates and trims a comment body, throwing <see cref="ValidationException"/>
    /// (mapped to HTTP 400) on empty or over-long input.
    /// </summary>
    public static string Normalize(string? body)
    {
        var errors = new List<string>();
        var trimmed = body?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
            errors.Add("Comment body is required.");
        else if (trimmed.Length > Comment.MaxLength)
            errors.Add($"Comment cannot exceed {Comment.MaxLength} characters.");

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return trimmed;
    }
}
