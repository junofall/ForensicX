using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Helpers
{
    internal class PartitionIdentifier
    {
        public PartitionIdentifier() { }
        public PartitionIdentifier(string name) { }
        public byte PartitionID{ get; set; }

        public const int EMPTY_PARTITION = 0x00; 
        public const int FAT12 = 0x01;
        public const int XENIX_ROOT = 0x02;
        public const 
        public const int PS2_RECOVERY_PARTITION = 0xFE;
        public const int XENIX_BAD_BLOCK_TABLE = 0xFF;

    }
}
