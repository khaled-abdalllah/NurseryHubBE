namespace NurseryHub.Nurseries;

public class NurseryMediaOptions
{
    public const string SectionName = "Media";

    /// <summary>Public base URL used to build logo URLs (e.g. http://localhost:4800).</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:4800";

    /// <summary>Physical folder where logo files are stored (e.g. C:\Media\logo).</summary>
    public string LogoPhysicalPath { get; set; } = @"C:\Media\logo";

    /// <summary>Allowed file extensions (lowercase, with dot).</summary>
    public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    public long MaxLogoBytes { get; set; } = 5 * 1024 * 1024;
}
