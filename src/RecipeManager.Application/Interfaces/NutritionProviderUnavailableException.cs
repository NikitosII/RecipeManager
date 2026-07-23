namespace RecipeManager.Application.Interfaces;

/// <summary>
/// Signals a transient failure of the nutrition source — a rate-limit (HTTP 429),
/// a server error (5xx), or a network/timeout problem.
/// </summary>
public class NutritionProviderUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
