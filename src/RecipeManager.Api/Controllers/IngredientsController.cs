using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Ingredients.Commands;
using RecipeManager.Application.Features.Ingredients.Queries;

namespace RecipeManager.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class IngredientsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetIngredientsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateIngredientRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateIngredientCommand(request.Name), cancellationToken);
        return Created(string.Empty, new { id });
    }
}

public record CreateIngredientRequest(string Name);
