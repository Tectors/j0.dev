using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Serilog;

namespace vj0.Shared.Extensions;

public static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetReadableSize(double size)
    {
        if (size <= 0) return "0 B";

        string[] suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        var order = 0;

        while (size >= 1024 && order < suffixes.Length - 1)
        {
            size /= 1024;
            order++;
        }

        return $"{size:0.##} {suffixes[order]}";
    }
    
    public static bool TryParseStringToDouble(string name, out double value)
    {
        var normalized = name;

        if (name.Count(c => c == '.') == 2)
        {
            var parts = name.Split('.');
            if (parts.Length == 3)
            {
                normalized = parts[0] + "." + parts[1] + parts[2];
            }
        }

        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            Log.Information($"Could not parse String {name} into double");
            return false;
        }

        return true;
    }
}