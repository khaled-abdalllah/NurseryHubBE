using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace NurseryHub.Nurseries;

public class UploadStudentImageInputValidator : AbstractValidator<UploadStudentImageInput>
{
    private static readonly HashSet<string> AllowedStudentExtensions =
        [".png", ".jpg", ".jpeg", ".webp"];

    private static readonly HashSet<string> AllowedStudentContentTypes =
        ["image/png", "image/jpeg", "image/jpg", "image/webp"];

    public UploadStudentImageInputValidator(IOptions<NurseryMediaOptions> mediaOptions)
    {
        var options = mediaOptions.Value;

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
            .Must(IsAllowedStudentImage)
            .When(x => x.File != null)
            .WithMessage("نوع الملف غير مدعوم");

        RuleFor(x => x.File!)
            .Must(f =>
                string.IsNullOrEmpty(f.ContentType) ||
                AllowedStudentContentTypes.Contains(f.ContentType.ToLowerInvariant()))
            .When(x => x.File != null)
            .WithMessage("نوع الملف غير مدعوم");
    }

    private static bool IsAllowedStudentImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext))
        {
            return false;
        }

        return AllowedStudentExtensions.Contains(ext.ToLowerInvariant());
    }
}
