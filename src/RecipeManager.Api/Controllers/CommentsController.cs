using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Comments.Commands;
using RecipeManager.Application.Features.Comments.Queries;

namespace RecipeManager.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public class CommentsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirst("sub")!.Value);

    private Guid? OptionalUserId => User.FindFirst("sub") is { } claim ? Guid.Parse(claim.Value) : null;

    [HttpGet("recipes/{recipeId:guid}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForRecipe(Guid recipeId, CancellationToken cancellationToken)
    {
        var comments = await mediator.Send(new GetCommentsForRecipeQuery(recipeId, OptionalUserId), cancellationToken);
        return Ok(comments);
    }

    [HttpPost("recipes/{recipeId:guid}/comments")]
    public async Task<IActionResult> Add(Guid recipeId, [FromBody] AddCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await mediator.Send(new AddCommentCommand(recipeId, CurrentUserId, request.Body), cancellationToken);
        return Ok(comment);
    }

    [HttpPut("comments/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await mediator.Send(new UpdateCommentCommand(id, CurrentUserId, request.Body), cancellationToken);
        return Ok(comment);
    }

    [HttpDelete("comments/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCommentCommand(id, CurrentUserId), cancellationToken);
        return NoContent();
    }
}

public record AddCommentRequest(string Body);

public record UpdateCommentRequest(string Body);
