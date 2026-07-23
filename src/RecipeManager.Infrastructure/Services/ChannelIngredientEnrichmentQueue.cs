using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using RecipeManager.Application.Interfaces;

namespace RecipeManager.Infrastructure.Services;

public sealed class ChannelIngredientEnrichmentQueue(ILogger<ChannelIngredientEnrichmentQueue> logger)
    : IIngredientEnrichmentQueue
{
    // Single reader, many writers.
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions { SingleReader = true });

    public ChannelReader<Guid> Reader => _channel.Reader;

    public void Enqueue(Guid ingredientId)
    {
        // TryWrite always succeeds on an unbounded channel that hasn't been completed.
        if (!_channel.Writer.TryWrite(ingredientId))
            logger.LogWarning("Could not enqueue ingredient {IngredientId} for nutrition enrichment.", ingredientId);
    }
}
