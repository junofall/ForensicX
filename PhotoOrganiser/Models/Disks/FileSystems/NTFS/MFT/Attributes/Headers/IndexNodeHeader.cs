using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers
{
    public class IndexNodeHeader
    {
        public uint OffsetToFirstIndexEntry { get; set; } //4 bytes
        public uint IndexEntriesTotalLength { get; set; } //4 bytes
        public uint AllocatedNodeSize { get; set; } //4 bytes
        public byte NonLeafNodeFlag { get; set; }  //1 byte
        public byte[] Padding { get; set; } //3 bytes

        //0x00 (Fits in Index Root) ; 0x01 (Needs Index Allocation) ; 
        // 0x01 indicates that $INDEX_ALLOCATION and $BITMAP are present, otherwise they are not.
        public IndexNodeHeader(BinaryReader reader)
        {
            OffsetToFirstIndexEntry = reader.ReadUInt32();
            IndexEntriesTotalLength = reader.ReadUInt32();
            AllocatedNodeSize = reader.ReadUInt32();
            NonLeafNodeFlag = reader.ReadByte();
            Padding = reader.ReadBytes(3);
        }
    }
}
