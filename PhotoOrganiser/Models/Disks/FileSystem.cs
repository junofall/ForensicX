using ForensicX.Interfaces;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks
{
    public abstract class FileSystem : IReadableFileSystem
    {
        public string Label { get; set; }
        public string SerialNumber { get; set; }
        public string Signature { get; set; }
        public ulong AllocationUnitSize { get; set; } // The size in bytes of whatever the filesystem is divided into. Chunks / Blocks / Clusters etc.
        public ulong TotalSize { get; set; }
        public ulong FreeSpace { get; set; }

        public abstract void LoadFileEntryData(FileEntry file);

        public abstract Task LoadFileEntryDataAsync(FileEntry file);

        public abstract void ExtractFile(FileEntry file, string directoryPath);
        public abstract void ExtractAll();

    }
}
