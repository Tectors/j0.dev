using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

using CUE4Parse.UE4.Versions;

namespace vj0.Core.Extensions;

public static class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        if (fi is null)
        {
            return $"{value} ({value:D})";
        }

        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes.Length > 0)
        {
            return attributes[0].Description;
        }

        var current = Convert.ToInt32(value);
        var mask = value.GetType() == typeof(EGame) ? ~0xFFFF : ~0xF;
        var target = current & mask;

        if (current != target)
        {
            var values = Enum.GetValues(value.GetType());
            var index = Array.IndexOf(values, value);
            var baseValue = values.GetValue(index - (current - target));
            return $"{value} ({baseValue})";
        }

        return $"{value} ({value:D})";
    }
}

public record EnumRecord(Enum Value, string Description)
{
    public override string ToString()
    {
        return Description;
    }
}

public class EnumToRecordConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumValue = value as Enum;
        
        if (enumValue is null) return null!;
        
        return new EnumRecord(enumValue, enumValue.GetDescription());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumValue = value as EnumRecord;
        return enumValue!.Value;
    }
}

public class EnumEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumValue = value as Enum;
        var compareValue = parameter as Enum;

        return enumValue!.Equals(compareValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumToItemsSource(Type type) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var values = Enum.GetValues(type).Cast<Enum>();
        return values.Select(value => new EnumRecord(value, value.GetDescription())).ToList();
    }
}
