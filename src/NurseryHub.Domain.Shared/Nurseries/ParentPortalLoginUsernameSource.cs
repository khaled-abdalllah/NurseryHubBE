namespace NurseryHub.Nurseries;

/// <summary>
/// Which parent's phone becomes the parent's login username (initial password is assigned when the account is created).
/// </summary>
public enum ParentPortalLoginUsernameSource
{
    FatherPhone = 1,
    MotherPhone = 2,
}
