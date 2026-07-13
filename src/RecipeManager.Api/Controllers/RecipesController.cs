using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Recipes.Commands;
using RecipeManager.Application.Features.Recipes.Queries;
using RecipeManager.Domain.Enums;

namespace RecipeManager.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RecipesController(IMediator mediator) : ControllerBase
{
    // The authenticated user's id, taken from the JWT "sub" claim. Only read on endpoints that require authorization.
    private Guid CurrentUserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetRecipesQuery(page, pageSize, search, categoryId),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRecipeByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CreateRecipeCommand(
                request.Title,
                request.Description,
                request.DifficultyLevel,
                request.PrepTimeMinutes,
                request.CookTimeMinutes,
                request.Servings,
                request.CategoryId,
                CurrentUserId),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecipeRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateRecipeCommand(
                id,
                request.Title,
                request.Description,
                request.DifficultyLevel,
                request.PrepTimeMinutes,
                request.CookTimeMinutes,
                request.Servings,
                CurrentUserId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteRecipeCommand(id, CurrentUserId), cancellationToken);
        return NoContent();
    }

    // -- Image upload -- //

    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "A non-empty file is required." });

        await using var stream = file.OpenReadStream();
        var imageUrl = await mediator.Send(
            new UploadRecipeImageCommand(id, stream, file.FileName, file.ContentType, file.Length, CurrentUserId),
            cancellationToken);

        return Ok(new { imageUrl });
    }

    // -- Step management -- //

    [HttpPost("{id:guid}/steps")]
    public async Task<IActionResult> AppendStep(Guid id, [FromBody] AppendStepRequest request, CancellationToken cancellationToken)
    {
        var step = await mediator.Send(
            new AppendRecipeStepCommand(id, request.Description, CurrentUserId),
            cancellationToken);
        return Ok(step);
    }

    [HttpPost("{id:guid}/steps/insert")]
    public async Task<IActionResult> InsertStep(Guid id, [FromBody] InsertStepRequest request, CancellationToken cancellationToken)
    {
        var step = await mediator.Send(
            new InsertRecipeStepCommand(id, request.AfterStepNumber, request.Description, CurrentUserId),
            cancellationToken);
        return Ok(step);
    }

    [HttpDelete("{id:guid}/steps/{stepNumber:int}")]
    public async Task<IActionResult> RemoveStep(Guid id, int stepNumber, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveRecipeStepCommand(id, stepNumber, CurrentUserId), cancellationToken);
        return NoContent();
    }

    // -- Ingredient management -- //

    [HttpPost("{id:guid}/ingredients")]
    public async Task<IActionResult> AddIngredient(Guid id, [FromBody] AddIngredientRequest request, CancellationToken cancellationToken)
    {
        var ingredient = await mediator.Send(
            new AddRecipeIngredientCommand(id, request.Name, request.Quantity, request.Unit, CurrentUserId),
            cancellationToken);
        return Ok(ingredient);
    }

    [HttpDelete("{id:guid}/ingredients/{ingredientId:guid}")]
    public async Task<IActionResult> RemoveIngredient(Guid id, Guid ingredientId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveRecipeIngredientCommand(id, ingredientId, CurrentUserId), cancellationToken);
        return NoContent();
    }
}

public record CreateRecipeRequest(
    string Title,
    string? Description,
    DifficultyLevel DifficultyLevel,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Guid CategoryId);

public record UpdateRecipeRequest(
    string Title,
    string? Description,
    DifficultyLevel DifficultyLevel,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings);

public record AppendStepRequest(string Description);

public record InsertStepRequest(int AfterStepNumber, string Description);

public record AddIngredientRequest(string Name, decimal Quantity, MeasurementUnit Unit);
