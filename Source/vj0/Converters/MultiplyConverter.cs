using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace vj0.Converters;

public class MultiplyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double input && parameter is string factorStr && double.TryParse(factorStr, out var factor))
        {
            return input * factor;
        }
        
        return value!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
