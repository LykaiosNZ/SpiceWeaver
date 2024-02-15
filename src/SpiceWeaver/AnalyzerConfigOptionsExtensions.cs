using System.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SpiceWeaver;

internal static class AnalyzerConfigOptionsExtensions
{
    public static bool TryGetNotNullOrEmptyValue(this AnalyzerConfigOptions options, string key, out string? value)
    {
        if (options.TryGetValue(key, out var foundValue) &&
            !string.IsNullOrEmpty(foundValue))
        {
            value = foundValue;
            Debug.Assert(value is not null);
            return true;
        }

        value = default;
        return false;
    }
}