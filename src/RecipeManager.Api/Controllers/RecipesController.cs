using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Application.Features.Ratings.Commands;
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

    // Same, but tolerant of anonymous callers — used on the [AllowAnonymous] read
    // endpoints to stamp the per-user "is favourite" flag when a token is present.
    private Guid? OptionalUserId =>
        User.FindFirst("sub") is { } claim ? Guid.Parse(claim.Value) : null;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] DifficultyLevel? difficulty = null,
        [FromQuery] int? maxPrepTime = null,
        [FromQuery] int? maxCookTime = null,
        [FromQuery] int? minServings = null,
        [FromQuery] Guid[]? ingredientIds = null,
        [FromQuery] RecipeSortBy sortBy = RecipeSortBy.DateCreated,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetRecipesQuery(
                page,
                pageSize,
                search,
                categoryId,
                difficulty,
                maxPrepTime,
                maxCookTime,
                minServings,
                ingredientIds,
                sortBy,
                sortDescending,
                OptionalUserId),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRecipeByIdQuery(id, OptionalUserId), cancellationToken);
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

    // -- Nutrition -- //

    [HttpPut("{id:guid}/nutrition")]
    public async Task<IActionResult> UpdateNutrition(Guid id, [FromBody] UpdateNutritionRequest request, CancellationToken cancellationToken)
    {
        var nutrition = await mediator.Send(
            new UpdateRecipeNutritionCommand(
                id,
                request.Mode,
                request.Calories,
                request.Protein,
                request.Fat,
                request.Carbohydrates,
                request.Fiber,
                CurrentUserId),
            cancellationToken);

        return Ok(nutrition);
    }

    // -- Ratings -- //

    [HttpPut("{id:guid}/rating")]
    public async Task<IActionResult> Rate(Guid id, [FromBody] RateRecipeRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RateRecipeCommand(id, CurrentUserId, request.Value), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/rating")]
    public async Task<IActionResult> RemoveRating(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveRecipeRatingCommand(id, CurrentUserId), cancellationToken);
        return NoContent();
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

public record UpdateNutritionRequest(
    NutritionMode Mode,
    decimal? Calories,
    decimal? Protein,
    decimal? Fat,
    decimal? Carbohydrates,
    decimal? Fiber);

public record RateRecipeRequest(int Value);

public record AppendStepRequest(string Description);

public record InsertStepRequest(int AfterStepNumber, string Description);

public record AddIngredientRequest(string Name, decimal Quantity, MeasurementUnit Unit);
