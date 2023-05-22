using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.Components
{
    public class NtfsVolumeBootRecord
    {
        public byte[] JumpInstruction { get; set; }
        public string OemId { get; set; }
        public ushort BytesPerSector { get; set; }
        public byte SectorsPerCluster { get; set; }
        public ushort ReservedSectors { get; set; }
        public byte[] Unused1 { get; set; }
        public ushort Unused2 { get; set; }
        public byte MediaDescriptor { get; set; }
        public ushort Unused3 { get; set; }
        public ushort SectorsPerTrack { get; set; }
        public ushort NumberOfHeads { get; set; }
        public uint HiddenSectors { get; set; }
        public uint Unused4 { get; set; }
        public uint Unused5 { get; set; }
        public ulong TotalSectors { get; set; }
        public ulong MftClusterNumber { get; set; }
        public ulong MftMirrClusterNumber { get; set; }
        public byte BytesOrClustersPerFileRecordSegment { get; set; }
        public byte[] Unused6 { get; set; }
        public byte BytesOrClustersPerIndexBuffer { get; set; }
        public byte[] Unused7 { get; set; }
        public ulong VolumeSerialNumber { get; set; }
        public uint Checksum { get; set; }
        public byte[] BootstrapCode { get; set; }
        public ushort EndOfSectorMarker { get; set; }
    }
}
