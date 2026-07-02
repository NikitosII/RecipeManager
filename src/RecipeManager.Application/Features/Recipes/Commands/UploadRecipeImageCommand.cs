using MediatR;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Recipes.Commands;

public record UploadRecipeImageCommand(
    Guid RecipeId,
    Stream Content,
    string FileName,
    string ContentType,
    long Length) : IRequest<string>;

public class UploadRecipeImageCommandHandler(
    IRecipeRepository recipeRepository,
    IFileStorageService fileStorage)
    : IRequestHandler<UploadRecipeImageCommand, string>
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public async Task<string> Handle(UploadRecipeImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0)
            throw new ValidationException(["File is empty."]);

        if (request.Length > MaxFileSizeBytes)
            throw new ValidationException([$"File exceeds the maximum size of {MaxFileSizeBytes / (1024 * 1024)} MB."]);

        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new ValidationException([$"Content type '{request.ContentType}' is not allowed. Permitted: {string.Join(", ", AllowedContentTypes)}."]);

        var recipe = await recipeRepository.GetByIdAsync(request.RecipeId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Domain.Entities.Recipe), request.RecipeId);

        // Remove the previously stored image, if any, to avoid orphaned files.
        if (!string.IsNullOrEmpty(recipe.ImageUrl))
        {
            var oldKey = Path.GetFileName(recipe.ImageUrl);
            if (!string.IsNullOrEmpty(oldKey))
                await fileStorage.DeleteAsync(oldKey, cancellationToken);
        }

        var blobKey = await fileStorage.UploadAsync(
            request.Content, request.FileName, request.ContentType, cancellationToken);

        var publicUrl = fileStorage.GetPublicUrl(blobKey);
        recipe.SetImageUrl(publicUrl);

        recipeRepository.Update(recipe);
        await recipeRepository.SaveChangesAsync(cancellationToken);

        return publicUrl;
    }
}
