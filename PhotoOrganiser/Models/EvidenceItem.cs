using CommunityToolkit.Mvvm.ComponentModel;
using ForensicX.Models.Disks;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models
{
    public abstract class EvidenceItem : ObservableObject
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public ObservableCollection<Partition> Children { get; set; } = new ObservableCollection<Partition>();

        public override string ToString()
        {
            return Name;
        }

        // Add any common properties or methods for all types of evidence items
    }
}
