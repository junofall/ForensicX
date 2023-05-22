using ForensicX.Models.Disks;
using ForensicX.Models.Disks.FileSystems.FAT16B;
using ForensicX.Models.Disks.FileSystems.FAT32;
using ForensicX.Models.Disks.FileSystems.NTFS;
using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace ForensicX.Models.Factory
{
    public static class FileSystemFactory
    {
        public static FileSystem CreateFileSystem(byte partitionType, Volume parentVolume)
        {
            FileSystem fileSystem;
            switch (partitionType)
            {
                case 0x06:
                    fileSystem = new FAT16BFileSystem(parentVolume);
                    return fileSystem;
                case 0x0C:
                    return null; // LBA
                case 0x0E:
                    fileSystem = new FAT16BFileSystem(parentVolume);
                    return fileSystem;
                case 0x07:
                    fileSystem = new NTFSFileSystem(parentVolume);
                    Debug.WriteLine("FileSystem Factory NTFS");
                    return fileSystem; // *Could* be either exFAT or NTFS.
                default:
                    //throw new NotSupportedException($"Partition type 0x{partitionType:X2} is not supported.");
                    return null;     
            }
        }
    }
}
