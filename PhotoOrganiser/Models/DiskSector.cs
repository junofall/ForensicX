using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models
{
    public class DiskSector
    {
        public long SectorNumber { get; set; }
        public bool IsAllocated { get; set; }
    }
}
