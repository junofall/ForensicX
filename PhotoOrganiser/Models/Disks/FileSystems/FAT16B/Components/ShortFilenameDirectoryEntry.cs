using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class ShortFilenameDirectoryEntry
    {
        public byte FirstByte { get; set; } // First character of the filename OR the allocation status (0x00 = unallocated, 0xE5 = Deleted).
        public string FileName { get; set; } // ASCII filename, the "." is implied between bytes 7 and 8.
        public byte Attributes { get; set; }
        public byte Reserved { get; set; }
        public byte CreationTimeDecisecond { get; set; } // in tenths of seconds
        public ushort CreationTime { get; set; }
        public ushort CreationDate { get; set; }
        public ushort AccessDate { get; set; }
        public ushort HighClusterAddress { get; set; } // FAT32 only!
        public ushort ModifiedTime { get; set; }
        public ushort ModifiedDate { get; set; }
        public ushort LowClusterAddress { get; set; } // This field is the first cluster address FAT12/16.
        public uint FileSize { get; set; }
    }
}
