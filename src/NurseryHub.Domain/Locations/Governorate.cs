using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace NurseryHub.Locations;

public class Governorate : Entity<Guid>
{
    public const int MaxNameLength = 128;
    public const int MaxCodeLength = 16;

    public string Code { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;

    protected Governorate()
    {
        Code = string.Empty;
        NameEn = string.Empty;
        NameAr = string.Empty;
    }

    public Governorate(Guid id, string code, string nameEn, string nameAr) : base(id)
    {
        SetCode(code);
        SetNameEn(nameEn);
        SetNameAr(nameAr);
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), MaxCodeLength);
    }

    public void SetNameEn(string nameEn)
    {
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), MaxNameLength);
    }

    public void SetNameAr(string nameAr)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), MaxNameLength);
    }
}
