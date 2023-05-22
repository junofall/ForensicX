using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers
{
    public abstract class StandardAttributeHeader
    {
        public uint AttributeType { get; set; }
        public uint AttributeLength { get; set; }
        public byte NonResidentFlag { get; set; }
        public byte NameLength { get; set; }
        public ushort OffsetToNameOrAttribute { get; set; }
        public ushort Flags { get; set; }
        public ushort AttributeID { get; set; }

        protected StandardAttributeHeader(byte[] attributeData)
        {
            AttributeType = BitConverter.ToUInt32(attributeData, 0x0);
            AttributeLength = BitConverter.ToUInt32(attributeData, 0x4);
            NonResidentFlag = attributeData[0x8];
            NameLength = attributeData[0x9];
            OffsetToNameOrAttribute = BitConverter.ToUInt16(attributeData, 0xA);
            Flags = BitConverter.ToUInt16(attributeData, 0xC);
            AttributeID = BitConverter.ToUInt16(attributeData, 0xE);
        }
    }
}
