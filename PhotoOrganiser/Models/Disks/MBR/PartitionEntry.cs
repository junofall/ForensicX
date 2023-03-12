using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.MBR
{
    public class PartitionEntry
    {
        public byte Bootable { get; set; }
        public Chs FirstSector { get; set; }
        public byte PartitionType { get; set; }
        public Chs FinalSector { get; set; }
        public uint FirstSectorLba { get; set; }
        public uint SectorCount { get; set; }

        public PartitionEntry(byte[] partitionBuffer)
        {
            Bootable = partitionBuffer[0];
            FirstSector = new Chs()
            {
                Head = partitionBuffer[1],
                Sector = (byte)(partitionBuffer[2] & 0x3F),
                Cylinder = (byte)(((partitionBuffer[2] & 0xC0) << 2) | partitionBuffer[3])
            };
            PartitionType = partitionBuffer[4];
            FinalSector = new Chs()
            {
                Head = partitionBuffer[5],
                Sector = (byte)(partitionBuffer[6] & 0x3F),
                Cylinder = (byte)(((partitionBuffer[6] & 0xC0) << 2) | partitionBuffer[7])
            };
            FirstSectorLba = BitConverter.ToUInt32(partitionBuffer, 8);
            SectorCount = BitConverter.ToUInt32(partitionBuffer, 12);
        }
    }

    public struct Chs
    {
        public byte Head { get; set; }
        public byte Sector { get; set; }
        public byte Cylinder { get; set; }
    }
}
