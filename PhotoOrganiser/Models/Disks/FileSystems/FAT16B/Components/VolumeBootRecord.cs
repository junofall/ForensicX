using ForensicX.Models.Disks.DOS_EBPB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class VolumeBootRecord : Dos40Ebpb
    {
        public byte[] X86JumpInstruction { get; set; }
        public char[] OemName { get; set; }

        //DOS 4.0 EBPB Fields

        public byte[] BootCode { get; set; }
        public ushort BootSectorSignature { get; set; }
    }
}
