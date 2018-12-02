using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monitor.Utilities
{
    class BasePriorityToComboBoxIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intPriority = (int)value;
             
            // not a switch case because priorities appear to be different from those 5 sometimes
            if (intPriority <= 4)  // Idle
                    return 0;
            if (intPriority <= 6)  // Below Norm
                    return 1;
            if (intPriority <= 8)  // Norm
                    return 2;
            if (intPriority <= 10)  // Above Norm
                    return 3;
            if (intPriority <= 13) // High
                    return 4;
            return 5; // Realtime

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
