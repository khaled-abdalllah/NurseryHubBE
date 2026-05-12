using System;

namespace NurseryHub.Nurseries;

/// <summary>
/// Parent contact row returned when searching by father or mother phone.
/// </summary>
public class ParentContactLookupDto
{
    public Guid Id { get; set; }
    public string FatherName { get; set; } = null!;
    public string FatherIdentityNumber { get; set; } = null!;
    public string FatherPhoneNumber { get; set; } = null!;
    public string MotherName { get; set; } = null!;
    public string MotherIdentityNumber { get; set; } = null!;
    public string MotherPhoneNumber { get; set; } = null!;

    /// <summary>
    /// True when an identity user already exists for the father's or mother's phone (same rule as parent portal login username).
    /// </summary>
    public bool HasParentPortalAccount { get; set; }
}
