using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class RootDirectoryRegion
    {
        public List<FileEntry> Entries { get; set; }
        public ulong StartSector { get; set; }

        public RootDirectoryRegion()
        {
            Entries = new List<FileEntry>();
        }
    }
}
