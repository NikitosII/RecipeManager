namespace RecipeManager.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobKey, CancellationToken cancellationToken = default);
    string GetPublicUrl(string blobKey);
}
