namespace RecipeManager.Application.DTOs;

public record UserProfileDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    int RecipeCount);
