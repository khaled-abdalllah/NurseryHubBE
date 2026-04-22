using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NurseryHub.Nurseries;

public class UploadNurseryLogoInput
{
    [FromForm]
    public IFormFile File { get; set; } = null!;
}
