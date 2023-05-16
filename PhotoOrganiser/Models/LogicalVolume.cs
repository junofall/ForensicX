using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models
{
    public class LogicalVolume
    {
        public string? DeviceID { get; set; }
        public string? VolumeName { get; set; }
        public string? FileSystem { get; set; } // NTFS, FAT etc.
        public ulong? Size { get; set; } // Size in bytes
        public ulong? FreeSpace { get; set; }
        public ulong? StartingOffset { get; set; } // Where the volume is located on the physical disk.
        public int? PercentageUsed { get; set; } // holder for % of used space. Rounded down to the nearest int in the viewmodel.
    }
}
