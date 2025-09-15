using System;
using System.Collections.Generic;
using System.Linq;

namespace vj0.Core.Extensions;

public static class MiscExtensions
{
    public static bool Filter(string input, string filter)
    {
        var filters = filter.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return filters.All(x => input.Contains(x, StringComparison.OrdinalIgnoreCase));
    }
    
    public static void RemoveAll<T>(this IList<T> list, Predicate<T> predicate)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (!predicate(list[i])) continue;
            
            list.RemoveAt(i);
        }
    }
}
