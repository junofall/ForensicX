using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes
{
    public class DataRun
    {
        public ulong StartCluster { get; private set; }
        public ulong Length { get; private set; }

        private DataRun(ulong startCluster, ulong length)
        {
            StartCluster = startCluster;
            Length = length;
        }

        public static List<DataRun> ParseDataRuns(byte[] attributeData, int dataRunOffset)
        {
            List<DataRun> runList = new List<DataRun>();
            int currentIndex = dataRunOffset;
            ulong currentCluster = 0;

            while (true)
            {
                byte header = attributeData[currentIndex];
                if (header == 0)
                {
                    break; // End of data runs
                }

                int offsetSize = header >> 4;
                int lengthSize = header & 0x0F;

                currentIndex++;

                ulong runLength = 0;
                for (int i = 0; i < lengthSize; i++)
                {
                    runLength |= (ulong)attributeData[currentIndex + i] << (8 * i);
                }

                currentIndex += lengthSize;

                long runOffset = 0;
                for (int i = 0; i < offsetSize - 1; i++)
                {
                    runOffset |= (long)attributeData[currentIndex + i] << (8 * i);
                }

                // Sign-extend the last byte
                runOffset |= (long)((sbyte)attributeData[currentIndex + offsetSize - 1]) << (8 * (offsetSize - 1));

                currentIndex += offsetSize;

                currentCluster = (ulong)((long)currentCluster + runOffset);
                runList.Add(new DataRun(currentCluster, runLength));

                if (currentIndex >= attributeData.Length)
                {
                    break;
                }
            }

            return runList;
        }
    }
}
