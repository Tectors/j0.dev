using System;

namespace vj0.Core.Utilities;

public static class TimeUtilities
{
    public static string GetRelativeTime(DateTime dateTime, DateTime? now = null)
    {
        var currentTime = (now ?? DateTime.Now).ToLocalTime();
        var timeSpan = currentTime - dateTime.ToLocalTime();

        if (timeSpan.TotalSeconds < 60)
        {
            return $"{(int)timeSpan.TotalSeconds} second{(timeSpan.TotalSeconds >= 2 ? "s" : "")} ago";
        }

        if (timeSpan.TotalMinutes < 60)
        {
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";
        }

        if (timeSpan.TotalHours < 24)
        {
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";
        }

        return timeSpan.TotalDays < 30
            ? $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago"
            : timeSpan.TotalDays < 365
                ? $"{(int)(timeSpan.TotalDays / 30)} month{((timeSpan.TotalDays / 30) >= 2 ? "s" : "")} ago"
                : $"{(int)(timeSpan.TotalDays / 365)} year{((timeSpan.TotalDays / 365) >= 2 ? "s" : "")} ago";
    }
}
