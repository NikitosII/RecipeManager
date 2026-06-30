using RecipeManager.Domain.Common;

namespace RecipeManager.Domain.Entities;

public class RecipeStep : BaseEntity
{
    protected RecipeStep() { }

    internal RecipeStep(Guid recipeId, int stepNumber, string description)
    {
        RecipeId = recipeId;
        StepNumber = stepNumber;
        Description = description;
    }

    public Guid RecipeId { get; private set; }
    public int StepNumber { get; internal set; }
    public string Description { get; private set; } = default!;

    public Recipe? Recipe { get; private set; }

    public void UpdateDescription(string description)
    {
        Description = description;
        DateUpdated = DateTime.UtcNow;
    }

    internal void IncrementStepNumber() => StepNumber++;
    internal void DecrementStepNumber() => StepNumber--;
}
