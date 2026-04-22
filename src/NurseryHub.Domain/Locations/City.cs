using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace NurseryHub.Locations;

public class City : Entity<Guid>
{
    public const int MaxNameLength = 128;
    public const int MaxCodeLength = 16;

    public Guid GovernorateId { get; private set; }
    public string Code { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;

    protected City()
    {
        Code = string.Empty;
        NameEn = string.Empty;
        NameAr = string.Empty;
    }

    public City(Guid id, Guid governorateId, string code, string nameEn, string nameAr) : base(id)
    {
        GovernorateId = governorateId;
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

    public void SetGovernorateId(Guid governorateId)
    {
        GovernorateId = governorateId;
    }
}
