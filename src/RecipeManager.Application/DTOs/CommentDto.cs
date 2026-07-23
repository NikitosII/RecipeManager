namespace RecipeManager.Application.DTOs;

public record CommentDto(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string? AuthorAvatarUrl,
    string Body,
    bool CanEdit,
    DateTime DateCreated,
    DateTime DateUpdated);
