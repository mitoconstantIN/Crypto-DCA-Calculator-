using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CryptoDCACalculator
{
    public class TotalRowFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && str.ToUpper() == "TOTAL")
                return FontAttributes.Bold;

            return FontAttributes.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
