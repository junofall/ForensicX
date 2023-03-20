using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Helpers
{
    public class PartitionNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int partitionNumber = (int)value;
            return $"Partition {partitionNumber}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
