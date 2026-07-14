using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Favorites.Commands;
using RecipeManager.Application.Features.Favorites.Queries;

namespace RecipeManager.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FavoritesController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetFavoritesQuery(CurrentUserId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{recipeId:guid}")]
    public async Task<IActionResult> Add(Guid recipeId, CancellationToken cancellationToken)
    {
        await mediator.Send(new AddFavoriteCommand(recipeId, CurrentUserId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{recipeId:guid}")]
    public async Task<IActionResult> Remove(Guid recipeId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveFavoriteCommand(recipeId, CurrentUserId), cancellationToken);
        return NoContent();
    }
}
