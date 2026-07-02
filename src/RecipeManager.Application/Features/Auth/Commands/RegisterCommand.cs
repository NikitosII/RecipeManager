using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Auth.Commands;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<AuthResponseDto>;

public class RegisterCommandHandler(IUserService userService, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (success, userClaims, errors) = await userService.RegisterAsync(
            request.FirstName, request.LastName, request.Email, request.Password, cancellationToken);

        if (!success || userClaims is null)
            throw new ValidationException(errors);

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
