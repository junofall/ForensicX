using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.FAT16B.Components
{
    public class FatRegion
    {
        public List<FatEntry> FatEntries { get; set; }
        public ulong StartSector { get; set; }

        public FatRegion()
        {
            FatEntries = new List<FatEntry>();
        }

        public List<ushort> BuildClusterChain(ushort firstCluster)
        {
            var clusterChain = new List<ushort>
            {
                firstCluster
            };

            var currentCluster = firstCluster;
            while (!FatEntries[currentCluster].IsEndOfChain)
            {
                currentCluster = FatEntries[currentCluster].Value;
                clusterChain.Add(currentCluster);
            }

            return clusterChain;
        }
    }
}
