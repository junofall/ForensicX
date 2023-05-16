using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models
{
    public class DiskPartitionEntry
    {
        public string DeviceID { get; set; }
        public string Type { get; set; }
        public bool Bootable { get; set; }
        public bool PrimaryPartition { get; set; }
        public ulong Size { get; set; }
        public ulong StartingOffset { get; set; }
        public List<LogicalVolume> Volumes { get; set;}
    }
}
