using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Users.Commands;
using RecipeManager.Application.Features.Users.Queries;

namespace RecipeManager.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    // The authenticated user's id, taken from the JWT "sub" claim.
    private Guid CurrentUserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyProfileQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("me/avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "A non-empty file is required." });

        await using var stream = file.OpenReadStream();
        var avatarUrl = await mediator.Send(
            new UploadAvatarCommand(CurrentUserId, stream, file.FileName, file.ContentType, file.Length),
            cancellationToken);

        return Ok(new { avatarUrl });
    }
}
