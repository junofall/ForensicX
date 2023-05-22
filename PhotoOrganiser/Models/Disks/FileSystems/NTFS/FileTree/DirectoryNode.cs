using ForensicX.Models.Disks.FileSystems.NTFS.MFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.FileTree
{
    public class DirectoryNode
    {
        public long MftReference { get; set; }
        public string Name { get; set; }
        public DirectoryNode Parent { get; set; }
        public List<MftFileRecord> FileRecords { get; set; }
        public List<DirectoryNode> Subdirectories { get; set; }

        public DirectoryNode()
        {
            FileRecords = new List<MftFileRecord>();
            Subdirectories = new List<DirectoryNode>();
        }
    }
}
