namespace RecipeManager.Application.Interfaces;

/// <summary>
/// Hands ingredient ids off to a background worker for nutrition enrichment, keeping the
/// slow, rate-limited USDA lookup off the request/response write path. 
/// </summary>
public interface IIngredientEnrichmentQueue
{
    void Enqueue(Guid ingredientId);
}
