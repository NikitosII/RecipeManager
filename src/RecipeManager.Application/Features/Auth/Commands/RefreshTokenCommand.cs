using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string Token) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, IUserService userService, ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        // Reuse detection: a token that was already rotated (revoked) is being presented again.
        // This signals theft, so revoke the whole rotation chain that descended from it
        if (token.IsRevoked)
        {
            await RevokeDescendantsAsync(token, cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Refresh token has been revoked. Please sign in again.");
        }

        if (token.IsExpired)
            throw new UnauthorizedException("Refresh token has expired. Please sign in again.");

        var userClaims = await userService.GetByIdAsync(token.UserId, cancellationToken)
            ?? throw new UnauthorizedException("User not found.");

        var (accessToken, accessExpiry) = tokenService.GenerateAccessToken(userClaims);
        var (newRefreshValue, newRefreshExpiry) = tokenService.GenerateRefreshToken();

        token.Revoke(replacedByToken: newRefreshValue);
        var newRefreshToken = new RefreshToken(userClaims.UserId, newRefreshValue, newRefreshExpiry);
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            accessExpiry,
            newRefreshValue,
            userClaims.UserId,
            userClaims.Email,
            userClaims.FirstName,
            userClaims.LastName);
    }

    /// <summary>
    /// Walks the ReplacedByToken chain from the reused token and revokes every
    /// still-active descendant, cutting off the compromised session family.
    /// </summary>
    private async Task RevokeDescendantsAsync(RefreshToken start, CancellationToken cancellationToken)
    {
        var current = start;
        while (current?.ReplacedByToken is not null)
        {
            var next = await refreshTokenRepository.GetByTokenAsync(current.ReplacedByToken, cancellationToken);
            if (next is null)
                break;

            if (!next.IsRevoked)
                next.Revoke();

            current = next;
        }
    }
}
