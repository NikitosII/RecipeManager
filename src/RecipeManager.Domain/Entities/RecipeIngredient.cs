using RecipeManager.Domain.Common;
using RecipeManager.Domain.Enums;

namespace RecipeManager.Domain.Entities;

public class RecipeIngredient : BaseEntity
{
    protected RecipeIngredient() { }

    internal RecipeIngredient(Guid recipeId, Guid ingredientId, decimal quantity, MeasurementUnit unit)
    {
        RecipeId = recipeId;
        IngredientId = ingredientId;
        Quantity = quantity;
        Unit = unit;
    }

    public Guid RecipeId { get; private set; }
    public Guid IngredientId { get; private set; }
    public decimal Quantity { get; private set; }
    public MeasurementUnit Unit { get; private set; }

    public Recipe? Recipe { get; private set; }
    public Ingredient? Ingredient { get; private set; }

    public void UpdateQuantity(decimal quantity, MeasurementUnit unit)
    {
        Quantity = quantity;
        Unit = unit;
        DateUpdated = DateTime.UtcNow;
    }
}
