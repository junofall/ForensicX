using Microsoft.UI.Xaml.Data;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Helpers
{
    public class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int percentage = System.Convert.ToInt32(value);
            return percentage >= 90 ? "#d9534f" : "#0275d8";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
