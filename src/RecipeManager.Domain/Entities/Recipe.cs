using RecipeManager.Domain.Common;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Domain.Entities;

public class Recipe : BaseEntity
{
    private readonly List<RecipeStep> _steps = [];
    private readonly List<RecipeIngredient> _recipeIngredients = [];

    protected Recipe() { }

    // EF uses the backing fields directly; ordering is done at the query level.
    public IReadOnlyList<RecipeStep> Steps => _steps;
    public IReadOnlyList<RecipeIngredient> RecipeIngredients => _recipeIngredients;

    public Recipe(
        string title,
        string? description,
        DifficultyLevel difficultyLevel,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        Guid categoryId,
        Guid userId)
    {
        Title = title;
        Description = description;
        DifficultyLevel = difficultyLevel;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Servings = servings;
        CategoryId = categoryId;
        UserId = userId;
    }


    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DifficultyLevel DifficultyLevel { get; private set; }
    public int PrepTimeMinutes { get; private set; }
    public int CookTimeMinutes { get; private set; }
    public int Servings { get; private set; }
    public string? ImageUrl { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    // -- Basic updates -- //

    public void Update(
        string title,
        string? description,
        DifficultyLevel difficultyLevel,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings)
    {
        Title = title;
        Description = description;
        DifficultyLevel = difficultyLevel;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Servings = servings;
        DateUpdated = DateTime.UtcNow;
    }

    public void SetImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl;
        DateUpdated = DateTime.UtcNow;
    }

    // -- Step management via LinkedList -- //
    /// <summary>
    /// Steps are stored as a List<RecipeStep> for EF Core persistence but
    /// all insert/remove operations use a LinkedList so that mid-sequence
    /// changes are O(1) node-pointer updates rather than O(n) array shifts.
    /// </summary>

    public LinkedList<RecipeStep> GetSteps() =>
        new(_steps.OrderBy(s => s.StepNumber));

    public RecipeStep AppendStep(string description)
    {
        var nextNumber = _steps.Count > 0 ? _steps.Max(s => s.StepNumber) + 1 : 1;
        var step = new RecipeStep(Id, nextNumber, description);
        _steps.Add(step);
        DateUpdated = DateTime.UtcNow;
        return step;
    }

    /// <summary>
    /// Inserts a new step immediately after <paramref name="afterStepNumber"/>,
    /// re-numbering subsequent steps via linked-list traversal.
    /// </summary>
    public RecipeStep InsertStepAfter(int afterStepNumber, string description)
    {
        var linked = GetSteps();
        var anchor = linked.FirstOrDefault(s => s.StepNumber == afterStepNumber)
                     ?? throw new NotFoundException(nameof(RecipeStep), afterStepNumber);

        var node = linked.Find(anchor)!;

        // Traverse forward and shift step numbers
        for (var cur = node.Next; cur != null; cur = cur.Next)
            cur.Value.IncrementStepNumber();

        var newStep = new RecipeStep(Id, afterStepNumber + 1, description);
        _steps.Add(newStep);
        DateUpdated = DateTime.UtcNow;
        return newStep;
    }

    /// <summary>
    /// Removes the step at <paramref name="stepNumber"/> and closes the gap
    /// in numbering via linked-list traversal.
    /// </summary>
    public void RemoveStep(int stepNumber)
    {
        var linked = GetSteps();
        var target = linked.FirstOrDefault(s => s.StepNumber == stepNumber)
                     ?? throw new NotFoundException(nameof(RecipeStep), stepNumber);

        var node = linked.Find(target)!;

        for (var cur = node.Next; cur != null; cur = cur.Next)
            cur.Value.DecrementStepNumber();

        _steps.Remove(target);
        DateUpdated = DateTime.UtcNow;
    }

    // -- Ingredient management -- //

    public void AddIngredient(Guid ingredientId, decimal quantity, MeasurementUnit unit)
    {
        if (_recipeIngredients.Any(r => r.IngredientId == ingredientId))
            throw new ConflictException($"Ingredient '{ingredientId}' is already on this recipe.");

        _recipeIngredients.Add(new RecipeIngredient(Id, ingredientId, quantity, unit));
        DateUpdated = DateTime.UtcNow;
    }

    public void RemoveIngredient(Guid ingredientId)
    {
        var ri = _recipeIngredients.FirstOrDefault(r => r.IngredientId == ingredientId)
                 ?? throw new NotFoundException(nameof(RecipeIngredient), ingredientId);

        _recipeIngredients.Remove(ri);
        DateUpdated = DateTime.UtcNow;
    }
}
