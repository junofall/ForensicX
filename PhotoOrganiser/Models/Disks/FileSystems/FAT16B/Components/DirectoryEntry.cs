using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class DirectoryEntry
    {
        public string Name { get; set; }
        public List<FileEntry> Files { get; set; }
        public List<DirectoryEntry> Directories { get; set; }

        public DirectoryEntry(string name)
        {
            Name = name;
            Files = new List<FileEntry>();
            Directories = new List<DirectoryEntry>();
        }
    }
}
