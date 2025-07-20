using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace vj0.Converters;

public class ForegroundExpandedConverter : IValueConverter
{
    private IBrush ExpandedBrush { get; } = Brush.Parse("#ffffff");
    private IBrush DefaultBrush { get; } = Brush.Parse("#dbdbdb");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? ExpandedBrush : DefaultBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}