using ForensicX.Models.Factory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks
{
    public class Volume
    {
        public Partition ParentPartition;
        public string Name { get; set; }
        public uint Size { get; set; }
        public FileSystem FileSystem { get; set; }

        public Volume(byte type, Partition parentPartition)
        {
            ParentPartition = parentPartition;
            Debug.WriteLine($"Creating FileSystem via Factory of type {type:X}");
            FileSystem = FileSystemFactory.CreateFileSystem(type, this);
        }

        public void UpdateVolumeName(string volumeName)
        {
            Name = volumeName;
            Debug.WriteLine("Volume Name Updated: " + volumeName);
        }
    }
}
