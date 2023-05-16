using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class LongFilenameDirectoryEntry
    {
        public byte SequenceNumber { get; set; }
        public string Name1 { get; set; }
        public byte Attribute { get; set; }
        public byte Reserved1 { get; set; }
        public byte Checksum { get; set; }
        public string Name2 { get; set; }
        public ushort Reserved2 { get; set; }
        public string Name3 { get; set; }
    }
}
