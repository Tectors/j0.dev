using System.Runtime.CompilerServices;

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
}