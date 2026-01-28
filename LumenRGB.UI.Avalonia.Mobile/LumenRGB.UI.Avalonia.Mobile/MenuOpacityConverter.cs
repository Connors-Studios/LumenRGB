using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LumenRGB.UI.Avalonia.Mobile
{
    public class MenuOpacityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isOpen = value is bool b && b;
            return isOpen ? 1.0 : 0.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
