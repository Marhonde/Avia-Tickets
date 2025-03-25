using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AviaTickets;

public class IntToVisibilityConverter : IValueConverter
{
    public static IntToVisibilityConverter Instance { get; } = new IntToVisibilityConverter();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int ostatok)
            return ostatok <= 0;
        
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}