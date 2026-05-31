using System;
using System.Collections.Generic;

namespace NurseryHub.Nurseries;

internal static class StudentFieldNormalizer
{
    private static readonly string[] AllowedToiletTraining =
        ["Independent", "Needs Assistance", "In Training"];

    private static readonly Dictionary<string, string> ToiletTrainingAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["مكتمل"] = "Independent",
            ["مستقل"] = "Independent",
            ["جاري"] = "In Training",
            ["قيد التدريب"] = "In Training",
            ["يحتاج مساعدة"] = "Needs Assistance",
        };

    public static string? NormalizeToiletTraining(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (AllowedToiletTraining.Contains(trimmed))
        {
            return trimmed;
        }

        if (ToiletTrainingAliases.TryGetValue(trimmed, out var mapped))
        {
            return mapped;
        }

        return trimmed;
    }

    public static bool IsAllowedToiletTraining(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalized = NormalizeToiletTraining(value);
        return normalized != null && AllowedToiletTraining.Contains(normalized);
    }
}
