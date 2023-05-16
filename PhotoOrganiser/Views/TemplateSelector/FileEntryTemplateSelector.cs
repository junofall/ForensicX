using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Views.TemplateSelector
{
    public class FileEntryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is FileEntry fileEntry)
            {
                return fileEntry.IsDirectory ? DirectoryTemplate : FileTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
