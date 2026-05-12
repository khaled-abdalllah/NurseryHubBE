using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace NurseryHub.Nurseries;

public class UploadStudentImageInputValidator : AbstractValidator<UploadStudentImageInput>
{
    public UploadStudentImageInputValidator(IOptions<NurseryMediaOptions> mediaOptions)
    {
        var options = mediaOptions.Value;
        var allowed = options.AllowedImageExtensions.Select(e => e.ToLowerInvariant()).ToHashSet();

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("نوع الملف غير مدعوم");

        RuleFor(x => x.File!)
            .Must(f => f.Length > 0)
            .When(x => x.File != null)
            .WithMessage("الملف فارغ.");

        RuleFor(x => x.File!)
            .Must(f => f.Length <= options.MaxStudentImageBytes)
            .When(x => x.File != null)
            .WithMessage("حجم الصورة يجب ألا يتجاوز 5 ميجا");

        RuleFor(x => x.File!)
            .Must(f => IsAllowedStudentImage(f, allowed))
            .When(x => x.File != null)
            .WithMessage("نوع الملف غير مدعوم");
    }

    private static bool IsAllowedStudentImage(IFormFile file, HashSet<string> allowedExtensions)
    {
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext.ToLowerInvariant()))
        {
            return false;
        }

        if (string.IsNullOrEmpty(file.ContentType))
        {
            return true;
        }

        return file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}
