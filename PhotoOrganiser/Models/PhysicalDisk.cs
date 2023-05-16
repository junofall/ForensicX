using System.Collections.Generic;


namespace ForensicX.Models
{
    public class PhysicalDisk
    {
        public string? DeviceID { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public ulong? Size { get; set; }
        public string? MediaType { get; set; }
        public uint? BytesPerSector { get; set; }
        public string? InterfaceType { get; set; }
        public uint Partitions { get; set; }
        public uint SectorsPerTrack { get; set; }
        public ulong TotalCylinders { get; set; }
        public uint TotalHeads { get; set; }
        public ulong TotalSectors { get; set; }
        public ulong TotalTracks { get; set; }
        public uint TracksPerCylinder { get; set; }
        public List<DiskPartitionEntry> PartitionEntries { get; set; }
    }
}
