using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Auth.Commands;

namespace RecipeManager.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password),
            cancellationToken);
        return Created(string.Empty, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Intentionally anonymous:
    /// This endpoint is intentionally not protected by authentication or authorization
    /// Possession of the refresh token is the proof of ownership, and an expired access token must not prevent revocation.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }
}

public record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
