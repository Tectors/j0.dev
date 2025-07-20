using System;
using System.Globalization;
using Avalonia.Data.Converters;
using vj0.Shared.Extensions;

namespace vj0.Converters.Enum;

public class EnumToStringConverter : IValueConverter
{
    public static readonly EnumToStringConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case null:
            {
                return null!;
            }

            case System.Enum e:
            {
                return e.GetDescription();
            }
            default:
            {
                var t = value.GetType();
                return t.IsValueType ? ((System.Enum) Activator.CreateInstance(t)!).GetDescription() : value;    
            }
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}