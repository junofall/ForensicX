﻿using ForensicX.Models.Disks.MBR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.WriteLine("Disk Created.");
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

        public async Task InitializeAsync()
        {
            using (var stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[SectorSize];
                await stream.ReadAsync(buffer, 0, (int)SectorSize).ConfigureAwait(false);

                // Parse the partition entries in the MBR
                for (int i = 0; i < 4; i++)
                {
                    var partitionBuffer = new byte[16];
                    Array.Copy(buffer, 446 + (i * 16), partitionBuffer, 0, 16);
                    var partitionEntry = new PartitionEntry(partitionBuffer);

                    // Check if the partition is an extended partition and parse its logical partitions
                    if (partitionEntry.PartitionType == 0x05 || partitionEntry.PartitionType == 0x0F)
                    {
                        Debug.WriteLine($"{partitionEntry.PartitionType:X} found. Finding additional entries...");
                        var firstEbaPhysicalSector = partitionEntry.FirstSectorLba;
                        await ParseEbrAsync(partitionEntry, partitionEntry.FirstSectorLba, firstEbaPhysicalSector);
                    }
                    else if(partitionEntry.PartitionType == 0x00)
                    {
                        //Empty Partition - Do nothing
                    }
                    else
                    {
                        var newPartition = new Partition(partitionEntry, this, partitionEntry.FirstSectorLba, partitionEntry.FirstSectorLba);
                        newPartition.PartitionLength = partitionEntry.SectorCount * SectorSize;
                        Partitions.Add(newPartition);
                        Debug.WriteLine($"Set new partition PhysicalSectorOffset to {partitionEntry.FirstSectorLba}");
                    }
                }
            }
        }

        private async Task ParseEbrAsync(PartitionEntry partitionEntry, ulong currentEbrLba, ulong firstEbaSector)
        {
            var FirstEbaSector = firstEbaSector;

            using (var stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[SectorSize];

                Debug.WriteLine($"Parsing EBR. Going to entries at: {(long)currentEbrLba * SectorSize} : Sector {currentEbrLba}");
                stream.Seek((long)currentEbrLba * SectorSize, SeekOrigin.Begin);
                await stream.ReadAsync(buffer, 0, (int)SectorSize).ConfigureAwait(false);

                // Parse the partition entries in the EBR
                for (int i = 0; i < 2; i++)
                {
                    var ebrPartitionBuffer = new byte[16];
                    Array.Copy(buffer, 446 + (i * 16), ebrPartitionBuffer, 0, 16);

                    var ebrPartitionEntry = new PartitionEntry(ebrPartitionBuffer);

                    if (ebrPartitionEntry.PartitionType == 0x00)
                    {
                        continue;
                    }
                    else if (ebrPartitionEntry.PartitionType == 0x05 || ebrPartitionEntry.PartitionType == 0x0F)
                    {
                        ulong nextRelativeEbrLba = ebrPartitionEntry.FirstSectorLba;

                        if (nextRelativeEbrLba != 0)
                        {
                            ulong nextAbsoluteEbrLba = firstEbaSector + nextRelativeEbrLba;
                            await ParseEbrAsync(ebrPartitionEntry, nextAbsoluteEbrLba, FirstEbaSector);
                        }
                    }
                    else
                    {
                        ulong logicalOffset = currentEbrLba;
                        var newPartition = new Partition(ebrPartitionEntry, this, ebrPartitionEntry.FirstSectorLba + currentEbrLba, logicalOffset);
                        newPartition.PartitionLength = partitionEntry.SectorCount * SectorSize;
                        Partitions.Add(newPartition);
                    }
                }
            }
        }
    }
}

