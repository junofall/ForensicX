using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models
{
    public class PhysicalDisk
    {
        public string? DeviceID { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public ulong? Size { get; set; }
        public string? MediaType { get; set; }
        public List<LogicalVolume>? LogicalVolumes { get; set; }
    }
}
