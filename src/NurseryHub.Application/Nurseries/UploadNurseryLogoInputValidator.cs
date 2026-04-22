using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace NurseryHub.Nurseries;

public class UploadNurseryLogoInputValidator : AbstractValidator<UploadNurseryLogoInput>
{
    public UploadNurseryLogoInputValidator(IOptions<NurseryMediaOptions> mediaOptions)
    {
        var options = mediaOptions.Value;
        var allowed = options.AllowedImageExtensions.Select(e => e.ToLowerInvariant()).ToHashSet();

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("A file is required.");

        RuleFor(x => x.File!)
            .Must(f => f.Length > 0)
            .When(x => x.File != null)
            .WithMessage("The file is empty.");

        RuleFor(x => x.File!)
            .Must(f => f.Length <= options.MaxLogoBytes)
            .When(x => x.File != null)
            .WithMessage($"The file must be at most {options.MaxLogoBytes} bytes.");

        RuleFor(x => x.File!)
            .Must(f => IsAllowedImage(f, allowed))
            .When(x => x.File != null)
            .WithMessage(
                "Only image files are allowed. Allowed extensions: " +
                string.Join(", ", options.AllowedImageExtensions) +
                ".");

        RuleFor(x => x.File!)
            .Must(f => string.IsNullOrEmpty(f.ContentType) || f.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .When(x => x.File != null)
            .WithMessage("Only image content types are allowed.");
    }

    private static bool IsAllowedImage(IFormFile file, HashSet<string> allowedExtensions)
    {
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext))
        {
            return false;
        }

        return allowedExtensions.Contains(ext.ToLowerInvariant());
    }
}
