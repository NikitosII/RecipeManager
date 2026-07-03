using Microsoft.Extensions.Options;
using RecipeManager.Application.Configuration;
using RecipeManager.Application.Interfaces;

namespace RecipeManager.Infrastructure.Services;

public class LocalFileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private const string UploadsFolder = "uploads";
    private readonly string _uploadsRoot = Path.Combine(options.Value.RootPath, UploadsFolder);

    public async Task<string> UploadAsync(
        Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_uploadsRoot);

        var ext = Path.GetExtension(fileName);
        var blobKey = $"{Guid.CreateVersion7()}{ext}";
        var filePath = Path.Combine(_uploadsRoot, blobKey);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return blobKey;
    }

    public Task DeleteAsync(string blobKey, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_uploadsRoot, blobKey);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string blobKey) => $"/{UploadsFolder}/{blobKey}";
}
