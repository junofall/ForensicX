using ForensicX.Models.Disks.FileSystems.FAT16B;
using ForensicX.Models.Disks.FileSystems.NTFS;
using ForensicX.Models.Disks.FileSystems.NTFS.Components;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT;
using ForensicX.Models.Disks.FileSystems.NTFS.FileTree;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using ForensicX.Helpers.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;

namespace ForensicX.Models.Disks.FileSystems.NTFS
{
    public class NTFSFileSystem : FileSystem
    {
        public Disk ParentDisk;                 // The containing Disk
        public Partition ParentPartition;       // The containing Partition
        public Volume ParentVolume;             // The containing Volume
        public NtfsVolumeBootRecord BootRecord; // The VBR
        public List<MftFileRecord> mftRecords;  // All files
        public WindowsVersion Version { get; private set; }
        // Properties and methods specific to FAT16 file system go here.
        // Do NTFS things here

        //Extraction Logic
        Dictionary<string, string> successfulExtractions = new Dictionary<string, string>(); //K = name, V = path
        List<ExtractionError> extractionErrors = new List<ExtractionError>();

        public NTFSFileSystem(Volume parentVolume)
        {
            Debug.WriteLine("Constructor @ NTFSFileSystem");
            ParentVolume = parentVolume;
            ParentPartition = ParentVolume.ParentPartition;
            ParentDisk = ParentPartition.ParentDisk;
            BootRecord = InitializeVbr();
            mftRecords = new List<MftFileRecord>();
            Version = WindowsVersion.Windows10;

            //PrintVbrInfo(BootRecord);
            ReadMft();

            //PrintParsedIndexRootEntries();

            //long mftTestRef = 24;

            //MftFileRecord test = FindMftFileRecordByReference(mftTestRef);

            //Debug.WriteLine("=======TEST======");
            //Debug.WriteLine(test.GetFileName());

            //foreach (MftFileRecord mfr in mftRecords)
            //{
            //FullExtraction(@"E:\DemoTestOutput\ForensicX\NTFS", mfr);
            //}

            // list of extraction errors for record keeping or reporting
            //foreach (var error in extractionErrors)
            //{
             //   Console.WriteLine($"Error in file {error.Filename}: {error.ErrorMessage}");
            //}

            //Finally, generate HTML Report
            //string reportFilename = "E:\\DemoTestOutput\\ForensicX\\NTFS\\ExtractionReport.html";
            //GenerateExtractionReport(successfulExtractions, extractionErrors);
        }

        public byte[] ReadDataByClusters(ulong clusterOffset, ulong clusterCount)
        {
            // Convert the cluster-based offset and length to sector-based ones
            ulong sectorOffset = clusterOffset * BootRecord.SectorsPerCluster;
            uint sectorCount = (uint)(clusterCount * BootRecord.SectorsPerCluster);

            // Call the GetSectorBytes method from the ParentDisk
            return ParentDisk.GetSectorBytes(sectorOffset, sectorCount);
        }

        public NtfsVolumeBootRecord InitializeVbr()
        {
            NtfsVolumeBootRecord vbr = new NtfsVolumeBootRecord();

            // Read the VBR sector data from disk
            var sectorData = new byte[ParentDisk.SectorSize];
            using (var stream = new FileStream(ParentDisk.ImagePath, FileMode.Open, FileAccess.Read))
            {
                ulong vbrSectorOffset;
                if (ParentPartition.Type == 0x05 || ParentPartition.Type == 0x0F)
                {
                    // Calculate the logical offset for the VBR within the extended partition
                    ulong extendedStartSector = ParentPartition.PhysicalSectorOffset;
                    ulong logicalStartSector = ParentPartition.LogicalSectorOffset;
                    ulong logicalOffset = logicalStartSector - extendedStartSector;

                    // Calculate the sector offset for the VBR
                    vbrSectorOffset = (ParentPartition.PhysicalSectorOffset + logicalOffset) * ParentDisk.SectorSize;
                }
                else
                {
                    // The VBR is located within the main partition
                    vbrSectorOffset = ParentPartition.PhysicalSectorOffset * ParentDisk.SectorSize;
                }

                stream.Seek((long)vbrSectorOffset, SeekOrigin.Begin);
                stream.Read(sectorData, 0, (int)ParentDisk.SectorSize);

                Console.WriteLine("Parsing NTFS VBR at offset: " + vbrSectorOffset + $" (Sector {vbrSectorOffset / ParentDisk.SectorSize})");
            }

            // Parse the sectorData byte array and populate the vbr fields
            vbr.JumpInstruction = new byte[] { sectorData[0], sectorData[1], sectorData[2] };
            vbr.OemId = Encoding.ASCII.GetString(sectorData, 3, 8);
            vbr.BytesPerSector = BitConverter.ToUInt16(sectorData, 11);
            vbr.SectorsPerCluster = sectorData[13];
            vbr.MediaDescriptor = sectorData[21];
            vbr.SectorsPerTrack = BitConverter.ToUInt16(sectorData, 24);
            vbr.NumberOfHeads = BitConverter.ToUInt16(sectorData, 26);
            vbr.HiddenSectors = BitConverter.ToUInt32(sectorData, 28);
            vbr.TotalSectors = BitConverter.ToUInt64(sectorData, 40);
            vbr.MftClusterNumber = BitConverter.ToUInt64(sectorData, 48);
            vbr.MftMirrClusterNumber = BitConverter.ToUInt64(sectorData, 56);
            vbr.BytesOrClustersPerFileRecordSegment = sectorData[64];
            vbr.BytesOrClustersPerIndexBuffer = sectorData[68];
            vbr.VolumeSerialNumber = BitConverter.ToUInt64(sectorData, 72);
            vbr.EndOfSectorMarker = BitConverter.ToUInt16(sectorData, 510);

            return vbr;
        }

        public void ReadMft()
        {
            ulong mftClusterNumber = BootRecord.MftClusterNumber;
            uint sectorsPerCluster = BootRecord.SectorsPerCluster;
            uint bytesPerSector = BootRecord.BytesPerSector;
            ulong partitionStartingOffset = ParentPartition.PhysicalSectorOffset * bytesPerSector;
            uint bytesPerFileRecord = BootRecord.BytesOrClustersPerFileRecordSegment;

            Debug.WriteLine("$MFT Cluster Number: " + mftClusterNumber);
            Debug.WriteLine("Sectors per Cluster: " + sectorsPerCluster);
            Debug.WriteLine("Bytes per Sector: " + bytesPerSector);

            // Calculate the byte offset of the $MFT cluster
            ulong mftByteOffset = (mftClusterNumber * sectorsPerCluster * bytesPerSector) + partitionStartingOffset;
            Debug.WriteLine("$MFT Byte Offset: " + mftByteOffset);
            int invalidCounter = 0;

            using (var stream = new FileStream(ParentDisk.ImagePath, FileMode.Open, FileAccess.Read))
            {
                uint entryIndex = 0;
                Debug.WriteLine(bytesPerFileRecord);
                while (invalidCounter < 10)
                {
                    byte[] mftData = new byte[1024];
                    long currentOffset = (long)(mftByteOffset + (entryIndex * 1024));

                    stream.Seek(currentOffset, SeekOrigin.Begin);
                    int bytesRead = stream.Read(mftData, 0, mftData.Length);

                    // Terminate the loop if no data is read or the read data is less than the expected size
                    if (bytesRead != 1024)
                    {
                        break;
                    }

                    // Print the mftData as a hex string
                    string mftDataHex = BitConverter.ToString(mftData).Replace("-", " ");

                    Debug.WriteLine("MFT DATA :::: " + mftDataHex);

                    MftFileRecord mftFileRecord = new MftFileRecord(mftData, this);
                    if (mftFileRecord.IsValid())
                    {
                        mftRecords.Add(mftFileRecord);
                    }
                    else
                    {
                        invalidCounter++;
                    }

                    entryIndex++;
                }
            }
        }


        public void PrintVbrInfo(NtfsVolumeBootRecord vbr)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("NTFS Volume Boot Record: ");
            Console.WriteLine("===================================");
            Console.WriteLine("Jump Instruction                   : " + BitConverter.ToString(vbr.JumpInstruction));
            Console.WriteLine("OEM ID                             : " + vbr.OemId);
            Console.WriteLine("Bytes Per Sector                   : " + vbr.BytesPerSector);
            Console.WriteLine("Sectors Per Cluster                : " + vbr.SectorsPerCluster);
            Console.WriteLine("Media Descriptor                   : " + vbr.MediaDescriptor);
            Console.WriteLine("Sectors Per Track                  : " + vbr.SectorsPerTrack);
            Console.WriteLine("Number Of Heads                    : " + vbr.NumberOfHeads);
            Console.WriteLine("Hidden Sectors                     : " + vbr.HiddenSectors);
            Console.WriteLine("Total Sectors                      : " + vbr.TotalSectors);
            Console.WriteLine("$MFT Cluster Number                : " + vbr.MftClusterNumber);
            Console.WriteLine("$MFTMirr Cluster Number            : " + vbr.MftMirrClusterNumber);
            Console.WriteLine("Clusters Per File Record Segment   : " + vbr.BytesOrClustersPerFileRecordSegment);
            Console.WriteLine("Clusters Per Index Buffer          : " + vbr.BytesOrClustersPerIndexBuffer);
            Console.WriteLine("Volume Serial Number               : " + vbr.VolumeSerialNumber.ToString("X16"));
            Console.WriteLine("End of Sector Marker               : " + vbr.EndOfSectorMarker.ToString("X4"));
            Console.WriteLine("===================================");
        }


        public void FullExtraction(string outputPath, MftFileRecord mftFileRecord)
        {
            Debug.WriteLine($"\nRecord ID: {mftFileRecord.RecordID}");
            DataAttribute dataAttribute = null;
            FileNameAttribute filenameAttribute = null;

            foreach (var attribute in mftFileRecord.Attributes)
            {
                if (attribute is DataAttribute)
                {
                    dataAttribute = attribute as DataAttribute;
                }
                else if (attribute is FileNameAttribute)
                {
                    filenameAttribute = attribute as FileNameAttribute;
                }

                if (dataAttribute != null && filenameAttribute != null)
                {
                    break;
                }
            }

            if (dataAttribute == null || dataAttribute.DataRuns == null)
            {
                return; // No $DATA attribute or empty data runs, skipping extraction
            }
            if (string.IsNullOrEmpty(filenameAttribute?.Filename))
            {
                return; // No filename, skipping extraction
            }

            try
            {
                // Create a new file on your local disk
                string outputFile = $@"{outputPath}\{filenameAttribute.Filename}";

                using (var ntfsStream = new FileStream(ParentDisk.ImagePath, FileMode.Open, FileAccess.Read))
                {
                    using (var localFileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        // Read and write data for each data run
                        foreach (var run in dataAttribute.DataRuns)
                        {
                            ulong partitionOffsetInBytes = ParentPartition.PhysicalSectorOffset * BootRecord.BytesPerSector;
                            ulong byteOffset = (run.StartCluster * ((ulong)BootRecord.SectorsPerCluster * BootRecord.BytesPerSector)) + partitionOffsetInBytes;
                            ulong lengthInBytes = run.Length * ((ulong)BootRecord.SectorsPerCluster * BootRecord.BytesPerSector);

                            Debug.WriteLine(@"\/\/EXTRACT INFO\/\/" + BootRecord.SectorsPerCluster);
                            Debug.WriteLine("BootRecord - Sectors per Cluster: " + BootRecord.SectorsPerCluster);
                            Debug.WriteLine("BootRecord - Bytes per Sector: " + BootRecord.BytesPerSector);
                            Debug.WriteLine($"DataRun StartCluster: {run.StartCluster}, Length: {run.Length}");
                            Debug.WriteLine($"Moving to ByteOffset: {byteOffset}, LengthInBytes: {lengthInBytes}\n");
                            Debug.WriteLine(@"\/\/\/\/\/\/\/\/\/\/" + BootRecord.SectorsPerCluster);
                            byte[] buffer = new byte[lengthInBytes];

                            ntfsStream.Seek((long)byteOffset, SeekOrigin.Begin);

                            int bytesRead = ntfsStream.Read(buffer, 0, buffer.Length);

                            localFileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                successfulExtractions.Add(filenameAttribute.Filename, outputFile);
            }
            catch (Exception ex)
            {
                extractionErrors.Add(new ExtractionError
                {
                    Filename = $@"{outputPath}\{filenameAttribute.Filename}",
                    ErrorMessage = ex.Message
                });
            }
        }

        private static string GetBase64Thumbnail(string filePath, int maxWidth, int maxHeight)
        {
            using (var image = System.Drawing.Image.FromFile(filePath))
            {
                double ratioX = (double)maxWidth / image.Width;
                double ratioY = (double)maxHeight / image.Height;
                double ratio = Math.Min(ratioX, ratioY);

                int newWidth = (int)(image.Width * ratio);
                int newHeight = (int)(image.Height * ratio);

                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);

                    using (var memoryStream = new MemoryStream())
                    {
                        newImage.Save(memoryStream, ImageFormat.Png);
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        private static bool HasValidImageExtension(string filename)
        {
            var validExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string fileExtension = Path.GetExtension(filename).ToLower();
            return validExtensions.Contains(fileExtension);
        }

        public void ConstructFilepaths(DirectoryNode node, string currentPath)
        {
            string nodePath = Path.Combine(currentPath, node.Name);

            foreach (var fileRecord in node.FileRecords)
            {
                fileRecord.FilePath = Path.Combine(nodePath, fileRecord.GetFileName());
            }

            foreach (var subdir in node.Subdirectories)
            {
                ConstructFilepaths(subdir, nodePath);
            }
        }

        public MftFileRecord FindMftFileRecordByReference(long mftReference)
        {
            return mftRecords.FirstOrDefault(r => r.RecordID == mftReference);
        }


        public void PrintParsedIndexRootEntries()
        {
            foreach (MftFileRecord record in mftRecords)
            {
                IndexRootAttribute indexRoot = record.Attributes.OfType<IndexRootAttribute>().FirstOrDefault();
                IndexAllocationAttribute indexAllocation = record.Attributes.OfType<IndexAllocationAttribute>().FirstOrDefault();

                if (indexRoot != null)
                {
                    Console.WriteLine($"MFT Record ID: {record.RecordID}");
                    Console.WriteLine($"Filename: {record.GetFileName()}");
                    Console.WriteLine("Index Root Entries:");

                    foreach (IndexEntry entry in indexRoot.IndexEntries)
                    {
                        Console.WriteLine($"  MFT Reference: {entry.FileReference}");
                        Console.WriteLine($"  Entry Length: {entry.LengthOfIndexEntry}");
                        Console.WriteLine($"  Flags: {entry.Flags}");
                        Console.WriteLine("  ---");
                    }
                }
                if (indexAllocation != null)
                {
                    Console.WriteLine($"MFT Record ID: {record.RecordID}");
                    Console.WriteLine($"Filename: {record.GetFileName()}");
                    Console.WriteLine("Index Root Entries:");

                    foreach (IndexEntry entry in indexAllocation.IndexEntries)
                    {
                        Console.WriteLine($"  MFT Reference: {entry.FileReference}");
                        Console.WriteLine($"  Entry Length: {entry.LengthOfIndexEntry}");
                        Console.WriteLine($"  Flags: {entry.Flags}");
                        Console.WriteLine("  ---");
                    }
                }
            }
        }


        public string GenerateExtractionReport(Dictionary<string, string> successfulExtractions, List<ExtractionError> extractionErrors)
        {
            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html lang=\"en\">");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("<meta charset=\"UTF-8\">");
            htmlBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            htmlBuilder.AppendLine("<title>ForensicX | Extraction Report</title>");
            htmlBuilder.AppendLine("<style>");
            htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
            htmlBuilder.AppendLine("h1, h2 { color: #333; }");
            htmlBuilder.AppendLine("ul { list-style: none; padding: 0; }");
            htmlBuilder.AppendLine("li { margin-bottom: 10px; }");
            htmlBuilder.AppendLine("</style>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine("<h1>ForensicX Extraction Report</h1>");

            htmlBuilder.AppendLine("<h2>");
            htmlBuilder.AppendLine(successfulExtractions.Count.ToString());
            htmlBuilder.AppendLine(" Successful Extractions</h2>");
            if (successfulExtractions.Count > 0)
            {
                htmlBuilder.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\">");
                htmlBuilder.AppendLine("<tr><th>Preview</th><th>Name</th><th>Path</th></tr>");
                foreach (KeyValuePair<string, string> successfulExtraction in successfulExtractions)
                {
                    string filename = successfulExtraction.Key;
                    string path = successfulExtraction.Value;

                    htmlBuilder.AppendLine("<tr>");
                    if (HasValidImageExtension(filename))
                    {
                        string thumbnail = GetBase64Thumbnail(path, 250, 250);
                        htmlBuilder.AppendLine($"<td><img src=\"data:image/jpeg;base64,{thumbnail}\" alt=\"Thumbnail\" /></td>");
                    }
                    else
                    {
                        htmlBuilder.AppendLine("<td></td>");
                    }

                    htmlBuilder.AppendLine($"<td>{filename}</td>");
                    htmlBuilder.AppendLine($"<td>{path}</td>");
                    htmlBuilder.AppendLine("</tr>");
                }
                htmlBuilder.AppendLine("</table>");
            }
            else
            {
                htmlBuilder.AppendLine("<p>No successful extractions.</p>");
            }

            htmlBuilder.AppendLine("<h2>");
            htmlBuilder.AppendLine(extractionErrors.Count.ToString());
            htmlBuilder.AppendLine(" Extraction Errors</h2>");
            if (extractionErrors.Count > 0)
            {
                htmlBuilder.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\">");
                htmlBuilder.AppendLine("<tr><th>Filename</th><th>Error Message</th></tr>");

                foreach (var error in extractionErrors)
                {
                    htmlBuilder.AppendLine("<tr>");
                    htmlBuilder.AppendLine($"<td>{error.Filename}</td>");
                    htmlBuilder.AppendLine($"<td>{error.ErrorMessage}</td>");
                    htmlBuilder.AppendLine("</tr>");
                }

                htmlBuilder.AppendLine("</table>");
            }
            else
            {
                htmlBuilder.AppendLine("<p>No extraction errors found.</p>");
            }

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }

        public override void LoadFileEntryData(FileEntry file) { }

        public override Task LoadFileEntryDataAsync(FileEntry file) { return null; }

        public override void ExtractFile(FileEntry file, string directoryPath)
        {
            foreach (MftFileRecord mfr in mftRecords)
            {
            FullExtraction(directoryPath, mfr);
            }

            // list of extraction errors for record keeping or reporting
            foreach (var error in extractionErrors)
            {
               Console.WriteLine($"Error in file {error.Filename}: {error.ErrorMessage}");
            }

            //Finally, generate HTML Report
            string reportFilename = directoryPath += "\\ExtractionReport.html";
            GenerateExtractionReport(successfulExtractions, extractionErrors);
        }

        public override void ExtractAll()
        {
            throw new NotImplementedException();
        }
    }

    
}
