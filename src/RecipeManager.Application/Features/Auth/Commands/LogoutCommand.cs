using MediatR;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Idempotent: revoke the token if it exists and is still active; otherwise do nothing.
        var token = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (token is not null && !token.IsRevoked)
        {
            token.Revoke();
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
