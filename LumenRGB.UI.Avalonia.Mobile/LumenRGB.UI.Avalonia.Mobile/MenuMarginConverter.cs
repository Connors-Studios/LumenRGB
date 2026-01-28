using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LumenRGB.UI.Avalonia.Mobile
{
    public class MenuMarginConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isOpen = value is bool b && b;

            // Closed → slide left off-screen
            if (!isOpen)
                return new Thickness(-150, 10, 0, 10);

            // Open → normal position
            return new Thickness(10, 10, 0, 10);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
