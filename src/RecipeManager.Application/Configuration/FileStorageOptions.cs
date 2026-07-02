namespace RecipeManager.Application.Configuration;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Absolute path of the web root the files are stored under and served from.
    /// Populated at startup from IWebHostEnvironment.WebRootPath so that the directory written to always matches what UseStaticFiles serves.
    /// </summary>
    public string RootPath { get; set; } = default!;

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // ~10 MB
}
