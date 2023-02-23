using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace ForensicX.Helpers
{
    public class PercentageUsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is ulong) || (ulong)value == 0 || parameter == null || !(parameter is ulong) || (ulong)parameter == 0)
            {
                return 0;
            }

            var usedSpace = (ulong)value;
            var totalSpace = (ulong)parameter;

            return (int)((usedSpace * 100) / totalSpace);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
