using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace vj0.Extensions;

public static class ComparisonExtensions
{
    public static int CompareNatural(string x, string y)
    {
        if (x == y)
        {
            return 0;
        }

        var xParts = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
        var yParts = Regex.Split(y.Replace(" ", ""), "([0-9]+)");

        var length = Math.Min(xParts.Length, yParts.Length);
        for (var i = 0; i < length; i++)
        {
            if (xParts[i] != yParts[i])
            {
                return PartCompare(xParts[i], yParts[i]);
            }
        }

        return xParts.Length.CompareTo(yParts.Length);
    }

    private static int PartCompare(string left, string right)
    {
        var leftIsNumber = int.TryParse(left, out var x);
        var rightIsNumber = int.TryParse(right, out var y);

        if (leftIsNumber && rightIsNumber)
        {
            return x.CompareTo(y);
        }

        return string.Compare(left, right, StringComparison.Ordinal);
    }
}

public class CustomComparer<T>(Comparison<T> comparison) : IComparer<T>
{
    public int Compare(T? x, T? y) => comparison(x!, y!);
}
