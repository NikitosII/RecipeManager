using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RecipeManager.Application.Features.Ingredients;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Infrastructure.Services;

/// <summary>
/// Drains the <see cref="ChannelIngredientEnrichmentQueue"/> and looks each ingredient's
/// nutrition up from the provider, off the request/response path.
/// </summary>
public sealed class IngredientEnrichmentWorker(ChannelIngredientEnrichmentQueue queue, IServiceScopeFactory scopeFactory, ILogger<IngredientEnrichmentWorker> logger) : BackgroundService
{
    private const int MaxAttempts = 4;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var ingredientId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await EnrichWithRetryAsync(ingredientId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Never let one bad item tear down the worker loop.
                logger.LogError(ex, "Nutrition enrichment failed for ingredient {IngredientId}.", ingredientId);
            }
        }
    }

    private async Task EnrichWithRetryAsync(Guid ingredientId, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            // A fresh scope per attempt: the repository and provider are scoped services.
            await using var scope = scopeFactory.CreateAsyncScope();
            var ingredients = scope.ServiceProvider.GetRequiredService<IIngredientRepository>();
            var provider = scope.ServiceProvider.GetRequiredService<INutritionProvider>();

            var ingredient = await ingredients.GetByIdAsync(ingredientId, cancellationToken);
            if (ingredient is null)
            {
                logger.LogDebug("Ingredient {IngredientId} no longer exists; skipping enrichment.", ingredientId);
                return;
            }

            if (ingredient.HasNutrition)
                return; // Already enriched (e.g. by an interactive refresh) — nothing to do.

            try
            {
                if (await IngredientEnrichment.EnrichAsync(ingredient, provider, cancellationToken))
                {
                    ingredients.Update(ingredient);
                    await ingredients.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Enriched nutrition for '{Name}' ({IngredientId}).", ingredient.Name, ingredientId);
                }
                else
                {
                    logger.LogDebug("No nutrition data found for '{Name}' ({IngredientId}).", ingredient.Name, ingredientId);
                }

                return; // Success (data applied) or a definitive "no data" — either way, done.
            }
            catch (NutritionProviderUnavailableException ex) when (attempt < MaxAttempts)
            {
                var delay = BaseDelay * Math.Pow(2, attempt - 1); // 2s, 4s, 8s…
                logger.LogWarning(
                    "Nutrition source unavailable for '{Name}' (attempt {Attempt}/{Max}): {Reason}. Retrying in {Delay}.",
                    ingredient.Name, attempt, MaxAttempts, ex.Message, delay);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
