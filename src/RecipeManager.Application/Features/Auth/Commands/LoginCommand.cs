using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandHandler(IUserService userService, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (outcome, userClaims) = await userService.ValidateCredentialsAsync(
            request.Email, request.Password, cancellationToken);

        if (outcome == LoginOutcome.LockedOut)
            throw new UnauthorizedException(
                "Account locked due to multiple failed sign-in attempts. Please try again later.");

        if (outcome != LoginOutcome.Success || userClaims is null)
            throw new UnauthorizedException("Invalid email or password.");

        var (accessToken, accessExpiry) = tokenService.GenerateAccessToken(userClaims);
        var (refreshTokenValue, refreshExpiry) = tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken(userClaims.UserId, refreshTokenValue, refreshExpiry);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            accessExpiry,
            refreshTokenValue,
            userClaims.UserId,
            userClaims.Email,
            userClaims.FirstName,
            userClaims.LastName);
    }
}
