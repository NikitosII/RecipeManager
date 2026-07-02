namespace RecipeManager.Application.DTOs;

public record UserClaimsDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
