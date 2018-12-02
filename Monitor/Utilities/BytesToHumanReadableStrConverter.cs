using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Monitor.Utilities
{
    public class BytesToHumanReadableStrConverter : IValueConverter
    {
        public static string ConvertToHumanReadableString(long number)
        {
            List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < suffixes.Count; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return ((number / (int)Math.Pow(1024, i)) + suffixes[i]).ToString();
                }
            }

            return number.ToString();
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long number = (long)value;
            return ConvertToHumanReadableString(number);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
