﻿using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT32
{
    public class FAT32FileSystem : FileSystem
    {
        public Disk ParentDisk;             // The containing Disk
        public Partition ParentPartition;   // The containing Partition
        public Volume ParentVolume;         // The containing Volume
        // Properties and methods specific to FAT16 file system go here.

        public FAT32FileSystem(Volume parentVolume)
        {
            ParentVolume = parentVolume;
            ParentPartition = ParentVolume.ParentPartition;
            ParentDisk = ParentPartition.ParentDisk;
        }

        public override void ExtractAll()
        {
            throw new NotImplementedException();
        }

        public override void ExtractFile(FileEntry file, string directoryPath)
        {
            throw new NotImplementedException();
        }

        public override void LoadFileEntryData(FileEntry file)
        {
            throw new NotImplementedException();
        }

        public override Task LoadFileEntryDataAsync(FileEntry file)
        {
            throw new NotImplementedException();
        }
    }
}
