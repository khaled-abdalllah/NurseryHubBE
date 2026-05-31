namespace NurseryHub.Nurseries;

public class NurseryMediaOptions
{
    public const string SectionName = "Media";

    /// <summary>Public base URL of this API host, used to build media URLs (must match how clients reach the API).</summary>
    public string PublicBaseUrl { get; set; } = "https://localhost:44301";

    /// <summary>Root physical folder where tenant media folders are stored (e.g. C:\Media).</summary>
    public string MediaRootPath { get; set; } = @"C:\Media";

    /// <summary>Allowed file extensions (lowercase, with dot).</summary>
    public string[] AllowedImageExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".jfif", ".jpe", ".png", ".gif", ".webp", ".bmp", ".tif", ".tiff", ".svg", ".ico", ".avif",
        ".heic", ".heif",
    ];

    public long MaxLogoBytes { get; set; } = 5 * 1024 * 1024;
    public long MaxStudentImageBytes { get; set; } = 5 * 1024 * 1024;
}
