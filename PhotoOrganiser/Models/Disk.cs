using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models
{
    public class Disk
    {
        public string? DeviceID { get; set; }
        public string? Name { get; set; }
        public string? VolumeName { get; set; }
        public string? Description { get; set; }
        public string? FileSystem { get; set; }
        public ulong? Size { get; set; }
        public ulong? FreeSpace { get; set; }
        public int? PercentageUsed { get; set; }
        public string? MediaType { get; set; }
        public string? serialNumber { get; set; }
        public string? Model { get; set; }
    }
}
