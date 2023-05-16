using ForensicX.Helpers;
using ForensicX.Interfaces;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WinRT;

namespace ForensicX.Models.Disks.FileSystems.FAT16B
{
    public class FAT16BFileSystem : FileSystem, IReadableFileSystem
    {
        public Disk ParentDisk;             // The containing Disk
        public Partition ParentPartition;   // The containing Partition
        public Volume ParentVolume;         // The containing Volume
        // Properties and methods specific to FAT16 file system go here.
        public VolumeBootRecord vbr;
        public FatRegion FatRegion;
        public RootDirectoryRegion RootDirectoryRegion;
        public DataRegion DataRegion;

        public FAT16BFileSystem(Volume parentVolume)
        {
            ParentVolume = parentVolume;
            parentVolume.Type = "FAT16B";
            ParentPartition = ParentVolume.ParentPartition;
            ParentDisk = ParentPartition.ParentDisk;

            Debug.WriteLine("Initializing FAT16B Filesystem");
            vbr = InitializeVBR();
            FatRegion = InitializeFAT();
            RootDirectoryRegion = InitializeRootDirectory();
            DataRegion = new DataRegion()
            {
                StartSector = RootDirectoryRegion.StartSector + ((ulong)vbr.RootEntryCount * 32 / ParentDisk.SectorSize)
            };

            foreach(FileEntry child in RootDirectoryRegion.Children)
            {
                ParentVolume.Children.Add(child);
            }
        }

        public VolumeBootRecord InitializeVBR()
        {
            var vbr = new VolumeBootRecord();

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

                Console.WriteLine("Parsing VBR at offset: " + vbrSectorOffset + $" (Sector {vbrSectorOffset / ParentDisk.SectorSize})");
            }



            // Parse jump instruction and OEM name fields
            vbr.X86JumpInstruction = sectorData.Take(3).ToArray();
            vbr.OemName = Encoding.ASCII.GetString(sectorData, 3, 8).TrimEnd('\0').ToCharArray();

            // Parse DOS 2.0 BPB fields
            vbr.BytesPerSector = BitConverter.ToUInt16(sectorData, 0x0B);
            vbr.SectorsPerCluster = sectorData[0x0D];
            vbr.ReservedSectorCount = BitConverter.ToUInt16(sectorData, 0x0E);
            vbr.NumberOfFats = sectorData[0x10];
            vbr.RootEntryCount = BitConverter.ToUInt16(sectorData, 0x11);
            vbr.TotalSectors16 = BitConverter.ToUInt16(sectorData, 0x13);
            vbr.MediaDescriptor = sectorData[0x15];
            vbr.SectorsPerFat16 = BitConverter.ToUInt16(sectorData, 0x16);
            vbr.PhysicalSectorsPerTrack = BitConverter.ToUInt16(sectorData, 0x18);
            vbr.NumberOfHeads = BitConverter.ToUInt16(sectorData, 0x1A);
            vbr.HiddenSectors = BitConverter.ToUInt32(sectorData, 0x1C);
            vbr.LargeTotalLogicalSectors = BitConverter.ToUInt32(sectorData, 0x20);
            vbr.PhysicalDriveNumber = sectorData[0x24];
            vbr.Reserved = sectorData[0x25];
            vbr.ExtendedBootSignature = sectorData[0x26];
            vbr.VolumeId = BitConverter.ToUInt32(sectorData, 0x27);
            vbr.VolumeLabel = Encoding.ASCII.GetString(sectorData, 0x2B, 11).TrimEnd();
            vbr.FileSystemType = Encoding.ASCII.GetString(sectorData, 0x36, 8).TrimEnd();
            vbr.BootSectorSignature = BitConverter.ToUInt16(sectorData, 0x1FE);
            vbr.LargeTotalLogicalSectors = 0;

            // Calculate the actual number of sectors in the volume based on the BPB fields
            if (vbr.TotalSectors16 == 0)
            {
                // Use the large total sector count for volumes with more than 65535 sectors
                vbr.LargeTotalLogicalSectors = vbr.SectorsPerFat16;
            }
            else
            {
                // Use the total sector count for volumes with less than or equal to 65535 sectors
                vbr.LargeTotalLogicalSectors = vbr.TotalSectors16 / vbr.SectorsPerFat16;
            }


            return vbr;
        }

        public FatRegion InitializeFAT()
        {
            var fatRegion = new FatRegion
            {
                StartSector = (uint)(ParentPartition.PhysicalSectorOffset + vbr.ReservedSectorCount)
            };

            var fatBytes = ParentDisk.GetSectorBytes(fatRegion.StartSector, vbr.SectorsPerFat16);

            // Parse each 2-byte entry into a FatEntry object
            for (int i = 0; i < fatBytes.Length; i += 2)
            {
                var value = BitConverter.ToUInt16(fatBytes, i);
                var fatEntry = new FatEntry { Value = value };
                fatRegion.FatEntries.Add(fatEntry);
            }

            return fatRegion;
        }

        public RootDirectoryRegion InitializeRootDirectory()
        {
            var rootDirectoryRegion = new RootDirectoryRegion
            {
                // Calculate the start sector of the root directory
                StartSector =
                ParentPartition.PhysicalSectorOffset
                + vbr.ReservedSectorCount
                + (ulong)(vbr.NumberOfFats * vbr.SectorsPerFat16)
            };

            // Read the sectors containing the directory entries
            byte[] directoryBytes = ParentDisk.GetSectorBytes(rootDirectoryRegion.StartSector, (uint)(vbr.RootEntryCount * 32 / ParentDisk.SectorSize));

            rootDirectoryRegion.Children.AddRange(GetFileEntries(directoryBytes));

            return rootDirectoryRegion;
        }

        public List<FileEntry> GetFileEntries(byte[] directoryBytes)
        {
            List<FileEntry> fileEntries = new();
            List<LongFilenameDirectoryEntry> LongEntryStash = new(); // Hold the previous long entries so we can collate them.

            for (int offset = 0; offset < directoryBytes.Length; offset += 32)
            {
                byte entryByte = directoryBytes[offset];

                if (entryByte == 0x00) // End of directory marker
                {
                    break;
                }
                else if (entryByte == 0x2E) // Dot entry (".")
                {
                    continue;
                }
                if (directoryBytes[offset + 0x0b] == 0x08)
                {
                    ParentVolume.UpdateVolumeName((char)entryByte + Encoding.ASCII.GetString(directoryBytes, offset + 1, 10).TrimEnd());
                    continue;
                }
                else if ((directoryBytes[offset + 0x0b] & 0x0F) == 0x0F)
                {
                    var extendedAttributeEntry = new LongFilenameDirectoryEntry
                    {
                        SequenceNumber = entryByte,
                        Attribute = directoryBytes[offset + 0x0b],
                        Checksum = directoryBytes[offset + 0x0d],

                        Name1 = Encoding.Unicode.GetString(directoryBytes, offset + 1, 10).TrimEnd('\0', '\uFFFF'),
                        Name2 = Encoding.Unicode.GetString(directoryBytes, offset + 14, 12).TrimEnd('\0', '\uFFFF'),
                        Name3 = Encoding.Unicode.GetString(directoryBytes, offset + 28, 4).TrimEnd('\0', '\uFFFF')
                    };
                    LongEntryStash.Add(extendedAttributeEntry);
                }
                else
                {
                    var shortFileName = (char)entryByte + Encoding.ASCII.GetString(directoryBytes, offset + 1, 7).TrimEnd();
                    var fileEntry = new FileEntry
                    {
                        FileSystem = this,
                        Name = "",
                        Extension = Encoding.ASCII.GetString(directoryBytes, offset + 8, 3).TrimEnd(),
                        Attributes = (FileEntry.FileAttributes)(FileAttributes)directoryBytes[offset + 11],
                        //Reserved = rootDirectoryBytes[offset + 12],
                        //CreationTimeDecisecond = rootDirectoryBytes[offset + 13],
                        CreationTime = FatTimeConverter.ConvertFatDateTimeToDateTime(BitConverter.ToUInt16(directoryBytes, offset + 16), BitConverter.ToUInt16(directoryBytes, offset + 14)),
                        LastAccessedTime = FatTimeConverter.ConvertFatDateToDate(BitConverter.ToUInt16(directoryBytes, offset + 18)),
                        LastModifiedTime = FatTimeConverter.ConvertFatDateTimeToDateTime(BitConverter.ToUInt16(directoryBytes, offset + 24), BitConverter.ToUInt16(directoryBytes, offset + 22)),
                        FileSize = BitConverter.ToUInt32(directoryBytes, offset + 28),

                        //Build FAT Cluster Chain
                        ClusterChain = FatRegion.BuildClusterChain(Convert.ToUInt16((BitConverter.ToUInt16(directoryBytes, offset + 20) << 16) + BitConverter.ToUInt16(directoryBytes, offset + 26))),
                        IsDirectory = ((directoryBytes[offset + 11] & (byte)FileAttributes.Directory) == (byte)FileAttributes.Directory),
                        IsReadOnly = ((directoryBytes[offset + 11] & (byte)FileAttributes.ReadOnly) == (byte)FileAttributes.ReadOnly),
                        IsHidden = ((directoryBytes[offset + 11] & (byte)FileAttributes.Hidden) == (byte)FileAttributes.Hidden),
                        IsSystem = ((directoryBytes[offset + 11] & (byte)FileAttributes.System) == (byte)FileAttributes.System),
                        IsArchive = ((directoryBytes[offset + 11] & (byte)FileAttributes.Archive) == (byte)FileAttributes.Archive),
                    };

                    if (entryByte == 0xE5) // Deleted entry
                    {
                        fileEntry.IsDeleted = true;
                    }

                    if (LongEntryStash.Count != 0)
                    {
                        LongEntryStash.Reverse();

                        // If the entry has long filenames, iterate through and concatenate them
                        string fullFileName = "";
                        foreach (var lfnEntry in LongEntryStash)
                        {
                            fullFileName += lfnEntry.Name1 + lfnEntry.Name2 + lfnEntry.Name3;
                        }
                        fileEntry.Name = fullFileName;
                        LongEntryStash.Clear();
                    }
                    else
                    {
                        fileEntry.Name = shortFileName + "." + fileEntry.Extension;
                    }
                    if (fileEntry.IsDirectory)
                    {
                        int lastDotIndex = fileEntry.Name.LastIndexOf('.');
                        if (lastDotIndex >= 0)
                        {
                            fileEntry.Name = string.Concat(fileEntry.Name.AsSpan(0, lastDotIndex), fileEntry.Name.AsSpan(lastDotIndex + 1));
                        }
                        ulong rootDirStartSec = ParentPartition.PhysicalSectorOffset + vbr.ReservedSectorCount + (ulong)(vbr.NumberOfFats * vbr.SectorsPerFat16);
                        ulong dataRegionStartSec = rootDirStartSec + ((ulong)vbr.RootEntryCount * 32 / ParentDisk.SectorSize);
                        ulong clusterToSector = (ulong)((fileEntry.ClusterChain[0] - 2) * vbr.SectorsPerCluster) + dataRegionStartSec;
                        // Read the sectors containing the directory entries
                        byte[] clusterBytes = ParentDisk.GetSectorBytes(clusterToSector, vbr.SectorsPerCluster);

                        fileEntry.Children = new List<FileEntry>();

                        //Recursively parse
                        //Console.WriteLine("Recursively parsing...");
                        fileEntry.Children.AddRange(GetFileEntries(clusterBytes));
                    }

                    //Debug.WriteLine("Adding Entry...");
                    fileEntries.Add(fileEntry);
                }
            }
            return fileEntries;
        }

        public static void PrintVbr(VolumeBootRecord vbr)
        {
            Console.WriteLine("==========VOLUME BOOT RECORD==========");
            Console.WriteLine($"X86JumpInstruction: {BitConverter.ToString(vbr.X86JumpInstruction)}");
            Console.WriteLine($"OemName: {new string(vbr.OemName)}");
            Console.WriteLine($"BytesPerSector: {vbr.BytesPerSector}");
            Console.WriteLine($"SectorsPerCluster: {vbr.SectorsPerCluster}");
            Console.WriteLine($"ReservedSectorCount: {vbr.ReservedSectorCount}");
            Console.WriteLine($"NumberOfFats: {vbr.NumberOfFats}");
            Console.WriteLine($"RootEntryCount: {vbr.RootEntryCount}");
            Console.WriteLine($"TotalSectors16: {vbr.TotalSectors16}");
            Console.WriteLine($"MediaDescriptor: 0x{vbr.MediaDescriptor:X}");
            Console.WriteLine($"SectorsPerFat16: {vbr.SectorsPerFat16}");
            Console.WriteLine($"PhysicalSectorsPerTrack: {vbr.PhysicalSectorsPerTrack}");
            Console.WriteLine($"NumberOfHeads: {vbr.NumberOfHeads}");
            Console.WriteLine($"HiddenSectors: {vbr.HiddenSectors}");
            Console.WriteLine($"LargeTotalLogicalSectors: {vbr.LargeTotalLogicalSectors}");
            Console.WriteLine($"PhysicalDriveNumber: 0x{vbr.PhysicalDriveNumber:X}");
            Console.WriteLine($"CurrentHead: 0x{vbr.Reserved:X}");
            Console.WriteLine($"BootSignature: 0x{vbr.ExtendedBootSignature:X}");
            Console.WriteLine($"VolumeId: 0x{vbr.VolumeId:X}");
            Console.WriteLine($"VolumeLabel: {vbr.VolumeLabel}");
            Console.WriteLine($"FileSystemType: {vbr.FileSystemType}");
            Console.WriteLine("=====================================");
            //Console.WriteLine($"BootCode: {BitConverter.ToString(vbr.BootCode)}");
            //Console.WriteLine($"BootSectorSignature: 0x{vbr.BootSectorSignature:X}");
        }

        public override void LoadFileEntryData(FileEntry file)
        {
            try
            {
                if (!file.IsDirectory)
                {
                    using var stream = new FileStream(ParentDisk.ImagePath, FileMode.Open, FileAccess.Read);
                    (byte[] fileData, _, _) = Fat16BFileExtractor.ExtractFileFromClusterChain(file.ClusterChain, DataRegion.StartSector, (uint)file.FileSize, stream, vbr, DataRegion.StartSector);

                    file.Data = fileData;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading file contents: {ex.Message}");
            }
        }

        public void ExtractFile(FileEntry file, ulong firstDataSector, string directoryPath)
        {
            try
            {
                if (file.IsDirectory)
                {
                    //Debug.WriteLine("ExtractFile@FAT16B - Directory Path: " + directoryPath);
                    directoryPath = Path.Combine(directoryPath, file.Name);
                    //Debug.WriteLine("ExtractFile@FAT16B - Directory Path Combined: " + directoryPath);

                    Directory.CreateDirectory(directoryPath);

                    if (file.Children.Count > 0)
                    {
                        foreach (FileEntry subFile in file.Children)
                        {
                            ExtractFile(subFile, firstDataSector, directoryPath);
                        }
                    }
                }
                else
                {
                    using var stream = new FileStream(ParentDisk.ImagePath, FileMode.Open, FileAccess.Read);
                    (byte[] fileData, string md5, string sha1) = Fat16BFileExtractor.ExtractFileFromClusterChain(file.ClusterChain, firstDataSector, (uint)file.FileSize, stream, vbr, DataRegion.StartSector);

                    //Debug.WriteLine("Buffer Hashes:");
                    //Debug.WriteLine("MD5: " + md5);
                    //Debug.WriteLine("SHA1: " + sha1);

                    //Debug.WriteLine("ExtractFile 'ELSE' @FAT16B - Directory Path: " + directoryPath);
                    //Debug.WriteLine("ExtractFile 'ELSE' @FAT16B - Directory Path Combined: " + directoryPath);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var filePath = Path.Combine(ParentVolume.Name, directoryPath, Path.GetFileName(file.Name));
                    //Debug.WriteLine("ExtractFile 'ELSE' @FAT16B - File Path: " + filePath);
                    //Debug.WriteLine($"SPC: {vbr.SectorsPerCluster}, BPS: {vbr.BytesPerSector}");
                    //Debug.WriteLine(filePath);
                    File.WriteAllBytes(filePath, fileData);

                    // Read the file back into a byte array
                    byte[] writtenFile = File.ReadAllBytes(filePath);

                    // Calculate the MD5 and SHA1 hash values for the written file
                    using var md5Hash = MD5.Create();
                    byte[] hashBytes = md5Hash.ComputeHash(writtenFile);
                    string writtenFileMd5 = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    //Debug.WriteLine($"Written File MD5: {writtenFileMd5}");


                    using var sha1Hash = SHA1.Create();
                    byte[] hashBytes2 = sha1Hash.ComputeHash(writtenFile);
                    string writtenFileSha1 = BitConverter.ToString(hashBytes2).Replace("-", "").ToLower();
                    //Debug.WriteLine($"Written File SHA1: {writtenFileSha1}");

                    // Compare the hash values of the original file buffer and the written file
                    if (md5 == writtenFileMd5 && sha1 == writtenFileSha1)
                    {
                        Debug.WriteLine("Hash values match. The file was written correctly.");
                    }
                    else
                    {
                        Debug.WriteLine("Hash values do not match. There was an error writing the file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting file: {ex.Message}");
            }
        }
    }
}
