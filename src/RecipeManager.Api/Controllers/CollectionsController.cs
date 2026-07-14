using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Collections.Commands;
using RecipeManager.Application.Features.Collections.Queries;

namespace RecipeManager.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CollectionsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCollectionsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCollectionByIdQuery(id, CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCollectionRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CreateCollectionCommand(request.Name, request.Description, CurrentUserId),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCollectionRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateCollectionCommand(id, request.Name, request.Description, CurrentUserId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCollectionCommand(id, CurrentUserId), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/recipes/{recipeId:guid}")]
    public async Task<IActionResult> AddRecipe(Guid id, Guid recipeId, CancellationToken cancellationToken)
    {
        await mediator.Send(new AddRecipeToCollectionCommand(id, recipeId, CurrentUserId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/recipes/{recipeId:guid}")]
    public async Task<IActionResult> RemoveRecipe(Guid id, Guid recipeId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveRecipeFromCollectionCommand(id, recipeId, CurrentUserId), cancellationToken);
        return NoContent();
    }
}

public record CreateCollectionRequest(string Name, string? Description);

public record UpdateCollectionRequest(string Name, string? Description);
