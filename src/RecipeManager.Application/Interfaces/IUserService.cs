using RecipeManager.Application.DTOs;

namespace RecipeManager.Application.Interfaces;

public interface IUserService
{
    Task<(bool Success, UserClaimsDto? User, IReadOnlyList<string> Errors)> RegisterAsync(
        string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default);

    Task<(LoginOutcome Outcome, UserClaimsDto? User)> ValidateCredentialsAsync(
        string email, string password, CancellationToken cancellationToken = default);

    Task<UserClaimsDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
