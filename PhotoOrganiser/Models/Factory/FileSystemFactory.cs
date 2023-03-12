using ForensicX.Models.Disks;
using ForensicX.Models.Disks.FileSystems.FAT16B;
using ForensicX.Models.Disks.FileSystems.FAT32;
using ForensicX.Models.Disks.FileSystems.NTFS;
using System;

namespace ForensicX.Models.Factory
{
    public static class FileSystemFactory
    {
        public static FileSystem CreateFileSystem(byte partitionType, Volume parentVolume)
        {
            switch (partitionType)
            {
                case 0x06:
                    return new FAT16BFileSystem(parentVolume);
                case 0x0C:
                    return new FAT32FileSystem(parentVolume); // LBA
                case 0x0E:
                    return new FAT16BFileSystem(parentVolume); //LBA
                case 0x07:
                    return new NTFSFileSystem(parentVolume); // Could be either exFAT or NTFS.
                default:
                    throw new NotSupportedException($"Partition type 0x{partitionType:X2} is not supported.");
            }
        }
    }
}
