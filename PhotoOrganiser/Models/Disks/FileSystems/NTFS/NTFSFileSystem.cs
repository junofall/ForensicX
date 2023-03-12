using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS
{
    public class NTFSFileSystem : FileSystem
    {
        public Disk ParentDisk;             // The containing Disk
        public Partition ParentPartition;   // The containing Partition
        public Volume ParentVolume;         // The containing Volume
        // Properties and methods specific to FAT16 file system go here.

        public NTFSFileSystem(Volume parentVolume)
        {
            ParentVolume = parentVolume;
            ParentPartition = ParentVolume.ParentPartition;
            ParentDisk = ParentPartition.ParentDisk;
        }


    }
}
