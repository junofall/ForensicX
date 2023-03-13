using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks
{
    public class Disk
    {
        public string ImagePath { get; set; }
        public string DiskName { get; set; }
        public uint SectorSize { get; set; }
        public List<Partition> Partitions { get; set; }

        public Disk(string imagePath, uint sectorSize = 512)
        {
            ImagePath = imagePath;
            DiskName = Path.GetFileName(imagePath);
            SectorSize = sectorSize;
            Partitions = new List<Partition>();
        }

        public byte[] GetSectorBytes(ulong offset, uint sectorCount)
        {
            using (var stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[SectorSize * sectorCount];

                stream.Seek((long)(offset * SectorSize), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(SectorSize * sectorCount));

                return buffer;
            }
        }
    }
}

