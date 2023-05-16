using ForensicX.Helpers;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B
{
    public static class Fat16BFileExtractor
    {
        public static (byte[], string, string) ExtractFileFromClusterChain(List<ushort> clusterChain, ulong firstDataSector, uint fileSize, Stream stream, VolumeBootRecord vbr, ulong dataRegionStart)
        {
            byte[] fileBuffer = new byte[fileSize];
            string md5Hash;
            string sha1Hash;

            int bytesRead = 0;
            using (var md5 = MD5.Create())
            using (var sha1 = SHA1.Create())
            {
                foreach (ushort cluster in clusterChain)
                {
                    ulong clusterToSector = (ulong)((cluster - 2) * vbr.SectorsPerCluster) + firstDataSector;
                    stream.Seek((long)clusterToSector * vbr.BytesPerSector, SeekOrigin.Begin);
                    int bytesToRead = (int)Math.Min(fileSize - bytesRead, vbr.BytesPerSector * (long)vbr.SectorsPerCluster);
                    int bytesReadThisCluster = stream.Read(fileBuffer, bytesRead, bytesToRead);
                    bytesRead += bytesReadThisCluster;
                    if (bytesRead >= fileSize)
                    {
                        Debug.WriteLine("File buffer broke at " + bytesRead + " with file size " + fileSize);
                        break;
                    }
                }

                md5.ComputeHash(fileBuffer);
                sha1.ComputeHash(fileBuffer);
                md5Hash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
                sha1Hash = BitConverter.ToString(sha1.Hash).Replace("-", "").ToLower();
            }

            if (bytesRead != fileSize)
            {
                Debug.WriteLine("\nFile buffer not fully read. Expected: " + fileSize + ", Actual: " + bytesRead);
            }

            return (fileBuffer, md5Hash, sha1Hash);
        }
    }
}
