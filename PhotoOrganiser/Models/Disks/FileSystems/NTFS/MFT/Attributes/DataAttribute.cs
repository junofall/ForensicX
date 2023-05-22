using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes
{
    public class DataAttribute : BaseAttribute
    {
        public List<DataRun> DataRuns { get; private set; }
        public byte[] Data { get; private set; }

        public DataAttribute(StandardAttributeHeader attributeHeader)
        {
            //Debug.WriteLine("DataAttribute: Datarun");
            if (attributeHeader is ResidentAttributeHeader residentHeader && residentHeader.AttributeData.Length != 0)
            {
                ParseResidentData(residentHeader);
            }
            else if (attributeHeader is NonResidentAttributeHeader nonResidentHeader)
            {
                DataRuns = nonResidentHeader.DataRuns;
            }
        }

        private void ParseResidentData(ResidentAttributeHeader attributeHeader)
        {
            int attributeContentOffset = attributeHeader.AttributeOffset;
            int dataSize = attributeHeader.DataLength;

            if (attributeContentOffset + dataSize > attributeHeader.AttributeData.Length)
            {
                Debug.WriteLine("==== EXCEPTION DETAILS ====");
                Debug.WriteLine($"AttributeContentOffset: {attributeContentOffset}");
                Debug.WriteLine($"DataSize: {dataSize}");
                Debug.WriteLine($"AttributeData Length: {attributeHeader.AttributeData.Length}");
                Debug.WriteLine("===========================");

                // Calculate the actual size of the data to copy
                dataSize = attributeHeader.AttributeData.Length - attributeContentOffset;
            }

            Data = new byte[dataSize];
            Array.Copy(attributeHeader.AttributeData, attributeContentOffset, Data, 0, dataSize);
        }

        public override void PrintAttribute()
        {
            Console.WriteLine("Attribute                   :    $DATA");

            if (Data != null)
            {
                Console.WriteLine($"Data Size (bytes)          :    {Data.Length}");
            }

            if (DataRuns != null && DataRuns.Count > 0)
            {
                Console.WriteLine($"Number of Data Runs         :    {DataRuns.Count}");
                Console.WriteLine("Data Runs                   :");

                int dataRunIndex = 1;
                foreach (DataRun dataRun in DataRuns)
                {
                    Console.WriteLine($"  Data Run {dataRunIndex}");
                    Console.WriteLine($"        Starting Cluster    :    {dataRun.StartCluster}");
                    Console.WriteLine($"        Length (clusters)   :    {dataRun.Length}");
                    dataRunIndex++;
                }
            }
        }
    }
}
