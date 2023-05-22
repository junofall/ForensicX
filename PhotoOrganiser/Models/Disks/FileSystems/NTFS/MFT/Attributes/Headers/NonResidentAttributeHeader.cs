using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers
{
    public class NonResidentAttributeHeader : StandardAttributeHeader
    {
        public long InitialVCN { get; set; }
        public long FinalVCN { get; set; }
        public ushort DataRunOffset { get; set; }
        public ushort CompressionUnitSize { get; set; }
        public uint Padding { get; set; }
        public long AllocatedSize { get; set; }
        public long RealSize { get; set; }
        public long InitializedSize { get; set; }
        public string AttributeName { get; set; }
        public List<DataRun> DataRuns { get; set; }

        public NonResidentAttributeHeader(byte[] attributeData) : base(attributeData)
        {
            InitialVCN = BitConverter.ToInt64(attributeData, 0x10);
            FinalVCN = BitConverter.ToInt64(attributeData, 0x18);
            DataRunOffset = BitConverter.ToUInt16(attributeData, 0x20);
            CompressionUnitSize = BitConverter.ToUInt16(attributeData, 0x22);
            Padding = BitConverter.ToUInt32(attributeData, 0x24);
            AllocatedSize = BitConverter.ToInt64(attributeData, 0x28);
            RealSize = BitConverter.ToInt64(attributeData, 0x30);
            InitializedSize = BitConverter.ToInt64(attributeData, 0x38);
            if (NameLength != 0)
            {
                AttributeName = Encoding.ASCII.GetString(attributeData, 0x40, NameLength * 2);
                DataRuns = DataRun.ParseDataRuns(attributeData, 0x40 + (NameLength * 2));
            }
            else
            {
                DataRuns = DataRun.ParseDataRuns(attributeData, (int)DataRunOffset);
            }

            foreach (var run in DataRuns)
            {
                //Console.WriteLine($"==DATA RUNS == \n Start Cluster: {run.Item1}, Length: {run.Item2}");
            }

        }
    }
}

