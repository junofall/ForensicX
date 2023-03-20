using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForensicX.Models;
using ForensicX.Models.Disks;

namespace ForensicX.Views.TemplateSelector
{
    public class PartitionItemContainerStyleSelector : StyleSelector
    {
        public Style PartitionStyle { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item is Partition)
            {
                return PartitionStyle;
            }
            return base.SelectStyleCore(item, container);
        }
    }

}
