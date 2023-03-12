using ForensicX.Models.Disks.DOS_EBPB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.DOS_BPB
{
    public class Dos331Bpb : Dos20Bpb
    {
        public uint PhysicalSectorsPerTrack { get; set; }
        public uint NumberOfHeads { get; set; }
        public uint HiddenSectors { get; set; }
        public uint LargeTotalLogicalSectors { get; set; }
    }
}
