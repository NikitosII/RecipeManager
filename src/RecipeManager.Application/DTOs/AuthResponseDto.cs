namespace RecipeManager.Application.DTOs;

public record AuthResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiry,
    string RefreshToken,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName);
