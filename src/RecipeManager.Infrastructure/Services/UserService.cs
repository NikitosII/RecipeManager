using Microsoft.AspNetCore.Identity;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Interfaces;
using RecipeManager.Infrastructure.Identity;

namespace RecipeManager.Infrastructure.Services;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    public async Task<(bool Success, UserClaimsDto? User, IReadOnlyList<string> Errors)> RegisterAsync(
        string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description).ToArray());

        var roles = await userManager.GetRolesAsync(user);
        return (true, new UserClaimsDto(user.Id, email, firstName, lastName, roles.ToArray()), []);
    }

    public async Task<(LoginOutcome Outcome, UserClaimsDto? User)> ValidateCredentialsAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return (LoginOutcome.InvalidCredentials, null);

        if (await userManager.IsLockedOutAsync(user))
            return (LoginOutcome.LockedOut, null);

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            // Increment the failed-attempt counter;
            // Identity locks the account automatically once MaxFailedAccessAttempts is reached.
            await userManager.AccessFailedAsync(user);
            return await userManager.IsLockedOutAsync(user)
                ? (LoginOutcome.LockedOut, null)
                : (LoginOutcome.InvalidCredentials, null);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        return (LoginOutcome.Success,
            new UserClaimsDto(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToArray()));
    }

    public async Task<UserClaimsDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user);
        return new UserClaimsDto(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToArray());
    }
}
