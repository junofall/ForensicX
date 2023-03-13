﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class FileEntry
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string FilePath { get; set; }
        public ulong FileSize { get; set; }

        public FileAttributes Attributes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessedTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public List<ushort> ClusterChain { get; set; }
        public List<FileEntry> Subdirectories { get; set; }

        [Flags]
        public enum FileAttributes : byte
        {
            ReadOnly = 0x01,
            Hidden = 0x02,
            System = 0x04,
            VolumeId = 0x08,
            Directory = 0x10,
            Archive = 0x20,
            Device = 0x40,
            Reserved = 0x80
        }
        public bool IsDeleted { get; set; }

        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsSystem { get; set; }
        public bool IsArchive { get; set; }

    }
}
