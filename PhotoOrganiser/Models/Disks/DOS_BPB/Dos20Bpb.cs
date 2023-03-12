using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.DOS_BPB
{
    public class Dos20Bpb
    {
        public ushort BytesPerSector { get; set; }
        public byte SectorsPerCluster { get; set; }
        public ushort ReservedSectorCount { get; set; }
        public byte NumberOfFats { get; set; }
        public ushort RootEntryCount { get; set; }
        public uint TotalSectors16 { get; set; }
        public byte MediaDescriptor { get; set; }
        public ushort SectorsPerFat16 { get; set; }
    }
}
