using ForensicX.Models.Disks.MBR;
using System.Diagnostics;

namespace ForensicX.Models.Disks
{
    public class Partition
    {
        public Disk ParentDisk { get; set; }
        public PartitionEntry PartitionEntry { get; set; }
        public int PartitionNumber { get; set; }
        public Volume Volume { get; set; }
        public byte Type { get; set; }
        public ulong LogicalSectorOffset { get; set; }
        public ulong PhysicalSectorOffset { get; set; } //So we can tell where Logical Sector 0 is.

        public Partition(PartitionEntry partitionEntry, Disk parentDisk, ulong physicalSectorOffset, ulong logicalSectorOffset)
        {
            ParentDisk = parentDisk;
            PartitionEntry = partitionEntry;
            PartitionNumber = ParentDisk.Partitions.Count + 1;
            Type = partitionEntry.PartitionType;

            PhysicalSectorOffset = physicalSectorOffset;
            LogicalSectorOffset = logicalSectorOffset;

            Debug.WriteLine($"Partition created at Sector: \nPhysical: {PhysicalSectorOffset}, Logical: {LogicalSectorOffset}");

            Volume = InitializeVolume(Type, this);
            Volume.Size = PartitionEntry.SectorCount;
        }

        private Volume InitializeVolume(byte type, Partition parentPartition)
        {
            return new Volume(type, parentPartition);
        }
    }
}
