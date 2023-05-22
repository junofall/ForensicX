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

                Debug.WriteLine("MD5: " + md5Hash);
                Debug.WriteLine("SHA1: " + sha1Hash);
                Debug.WriteLine("\n");

                //PrintByteArray(fileBuffer);
            }

            if (bytesRead != fileSize)
            {
                Debug.WriteLine("\nFile buffer not fully read. Expected: " + fileSize + ", Actual: " + bytesRead);
            }



            return (fileBuffer, md5Hash, sha1Hash);
        }




        public static async Task<(byte[], string, string)> ExtractFileFromClusterChainAsync(List<ushort> clusterChain, ulong firstDataSector, uint fileSize, Stream stream, VolumeBootRecord vbr, ulong dataRegionStart)
        {
            Debug.WriteLine("ExtractFileFromClusterChainAsync: Extracting data.");
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
                    int bytesReadThisCluster = await stream.ReadAsync(fileBuffer, bytesRead, bytesToRead);
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

                Debug.WriteLine("MD5: " + md5Hash);
                Debug.WriteLine("SHA1: " + sha1Hash);
                Debug.WriteLine("\n");

                //PrintByteArray(fileBuffer);
            }

            if (bytesRead != fileSize)
            {
                Debug.WriteLine("\nFile buffer not fully read. Expected: " + fileSize + ", Actual: " + bytesRead);
            }
            Debug.WriteLine("Async extract done.");
            return (fileBuffer, md5Hash, sha1Hash);
        }



        static void PrintByteArray(byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder asciiRepresentation = new StringBuilder();

            for (int i = 0; i < byteArray.Length; i++)
            {
                // Append the byte as a 2-character hex string
                sb.Append(byteArray[i].ToString("X2") + " ");

                // Append ASCII representation if the byte is a printable ASCII character; else append a dot
                if (byteArray[i] >= 32 && byteArray[i] <= 126)
                {
                    asciiRepresentation.Append((char)byteArray[i]);
                }
                else
                {
                    asciiRepresentation.Append(".");
                }

                // Print 16 bytes per line
                if ((i + 1) % 16 == 0)
                {
                    sb.Append("    " + asciiRepresentation.ToString());
                    Debug.WriteLine(sb.ToString());

                    // Clear StringBuilder instances for the next line
                    sb.Clear();
                    asciiRepresentation.Clear();
                }
            }

            // Print the remaining bytes if they are less than 16
            if (sb.Length > 0)
            {
                // Add extra spaces to align the ASCII representation column if the line is not complete
                sb.Append(new string(' ', (16 * 3 - sb.Length)) + "    " + asciiRepresentation.ToString());
                Debug.WriteLine(sb.ToString());
            }
        }
    }
}
