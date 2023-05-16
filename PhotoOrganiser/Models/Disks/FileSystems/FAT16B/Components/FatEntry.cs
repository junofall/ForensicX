using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class FatEntry
    {
        public ushort Value { get; set; }

        public bool IsEndOfChain => (Value & 0xFFFF) >= 0xFFF8; // FFF8 - FFFF end of chain marker.
        public bool IsFree => Value == 0x0000;
    }
}
