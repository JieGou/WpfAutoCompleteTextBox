using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCompleteTextBox.Converters
{
    internal class AutoCompleteTextFieldPaddingToPopupMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var padding = (Thickness)value;
            return new Thickness(padding.Left, 0, padding.Right, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
