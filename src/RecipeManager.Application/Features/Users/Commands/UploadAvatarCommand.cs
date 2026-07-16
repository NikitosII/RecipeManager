using MediatR;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Exceptions;

namespace RecipeManager.Application.Features.Users.Commands;

public record UploadAvatarCommand(
    Guid UserId,
    Stream Content,
    string FileName,
    string ContentType,
    long Length) : IRequest<string>;

public class UploadAvatarCommandHandler(
    IUserService userService,
    IFileStorageService fileStorage) : IRequestHandler<UploadAvatarCommand, string>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0)
            throw new ValidationException(["File is empty."]);

        if (request.Length > MaxFileSizeBytes)
            throw new ValidationException([$"File exceeds the maximum size of {MaxFileSizeBytes / (1024 * 1024)} MB."]);

        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new ValidationException([$"Content type '{request.ContentType}' is not allowed. Permitted: {string.Join(", ", AllowedContentTypes)}."]);

        var profile = await userService.GetProfileAsync(request.UserId, cancellationToken)
                      ?? throw new NotFoundException("User", request.UserId);

        // Remove the previously stored avatar, if any, to avoid orphaned files.
        if (!string.IsNullOrEmpty(profile.AvatarUrl))
        {
            var oldKey = Path.GetFileName(profile.AvatarUrl);
            if (!string.IsNullOrEmpty(oldKey))
                await fileStorage.DeleteAsync(oldKey, cancellationToken);
        }

        var blobKey = await fileStorage.UploadAsync(
            request.Content, request.FileName, request.ContentType, cancellationToken);

        var publicUrl = fileStorage.GetPublicUrl(blobKey);
        await userService.SetAvatarUrlAsync(request.UserId, publicUrl, cancellationToken);

        return publicUrl;
    }
}
