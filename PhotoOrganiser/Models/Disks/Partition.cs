using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using ForensicX.Models.Disks.MBR;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace ForensicX.Models.Disks
{
    public class Partition : INotifyPropertyChanged
    {
        // ...
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    
        public Disk ParentDisk { get; set; }
        public string Name { get; set; }
        public PartitionEntry PartitionEntry { get; set; }
        public int PartitionNumber { get; set; }
        public Volume Volume { get; set; }
        public byte Type { get; set; }
        public ulong LogicalSectorOffset { get; set; }
        public ulong PhysicalSectorOffset { get; set; } //So we can tell where Logical Sector 0 is.
        public ulong PartitionLength { get; set; }

        public Partition(PartitionEntry partitionEntry, Disk parentDisk, ulong physicalSectorOffset, ulong logicalSectorOffset)
        {
            ParentDisk = parentDisk;
            PartitionEntry = partitionEntry;

            PartitionNumber = ParentDisk.Partitions.Count + 1;

            Name = $"Partition {PartitionNumber}";
            
            Type = partitionEntry.PartitionType;

            PhysicalSectorOffset = physicalSectorOffset;
            LogicalSectorOffset = logicalSectorOffset;

            Debug.WriteLine($"Partition created at Sector: \nPhysical: {PhysicalSectorOffset}, Logical: {LogicalSectorOffset}");

            Volume = InitializeVolume(Type, this);
            Volume.Size = PartitionEntry.SectorCount;
            
        }

        public override string ToString()
        {
            return Name;
        }

        private static Volume InitializeVolume(byte type, Partition parentPartition)
        {
            return new Volume(type, parentPartition);
        }
    }
}
