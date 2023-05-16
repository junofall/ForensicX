using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks
{
    public class FileContent
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string FullName { get; set; }
        public byte[] Content { get; set; }
    }
}
