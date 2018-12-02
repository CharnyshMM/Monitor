using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monitor.Utilities
{
    class ProcessBasePriorityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intPriority = (int)value;
            switch (intPriority)
            {
                case 1:
                    return "Dynamic Idle";
                case 4:
                    return "Idle";
                case 6:
                    return "Below Normal";
                case 8:
                    return "Normal";
                case 9:
                case 10:
                    return "Above Normal";
                case 11:
                case 13:
                    return "Hign";
                case 24:
                    return "RealTime";
                case 31:
                    return "Critical";
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
