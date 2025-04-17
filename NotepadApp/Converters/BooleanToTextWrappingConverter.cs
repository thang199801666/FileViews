using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NotepadApp.Converters
{
    public class BooleanToTextWrappingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is TextWrapping wrap && wrap == TextWrapping.Wrap);
        }
    }
}