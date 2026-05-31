using System;

namespace NurseryHub.Nurseries;

public static class NurseryLogoUrlHelper
{
    public static string? BuildLogoUrl(string publicBaseUrl, Guid nurseryId, bool hasLogo)
    {
        if (!hasLogo)
        {
            return null;
        }

        var baseUrl = publicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/api/app/nursery/{nurseryId:D}/logo";
    }
}
