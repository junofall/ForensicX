using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Management;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ForensicX.Models;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ForensicX.Services;
using Windows.ApplicationModel.Core;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace ForensicX.ViewModels
{
    public class DeviceListViewModel : ObservableObject
    {
        public CoreApplicationView? CurrentView { get; set; }
        public ObservableCollection<PhysicalDisk> PhysicalDisks { get; }
        public ICommand LoadDisksCommand { get; }
        public ICommand ImageDiskCommand { get; }
        public Microsoft.UI.Dispatching.DispatcherQueue dispatcher { get; set; }

        private string _sourceDevicePath { get; set; }
        private string _destinationDevicePath { get; set; }

        public DeviceListViewModel()
        {
            PhysicalDisks = new ObservableCollection<PhysicalDisk>();
            LoadDisksCommand = new RelayCommand(LoadDisks);
            ImageDiskCommand = new RelayCommand<string>(async sourcePath => await ImageDisk(sourcePath));
            LoadDisks();
        }

        private void LoadDisks()
        {
            PhysicalDisks.Clear();
            string query = "SELECT * FROM Win32_DiskDrive"; // Manually validate query via wbemtest.exe, connect... to 'root\cimv2'

            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
            {
                foreach (var disk in results)
                {
                    if(disk != null)
                    {
                        var physicalDisk = new PhysicalDisk
                        {
                            DeviceID = (string)disk.GetPropertyValue("DeviceID"),
                            Model = (string)disk.GetPropertyValue("Model"),
                            SerialNumber = (string)disk.GetPropertyValue("SerialNumber"),
                            Size = (ulong)disk.GetPropertyValue("Size"),
                            LogicalVolumes = new List<LogicalVolume>(),
                            MediaType = (string)disk.GetPropertyValue("MediaType"),

                            BytesPerSector = (uint)disk.GetPropertyValue("BytesPerSector"),
                            InterfaceType = (string)disk.GetPropertyValue("InterfaceType"),
                            Partitions = (uint)disk.GetPropertyValue("Partitions"),
                            SectorsPerTrack = (uint)disk.GetPropertyValue("SectorsPerTrack"),
                            TotalCylinders = (ulong)disk.GetPropertyValue("TotalCylinders"),
                            TotalHeads = (uint)disk.GetPropertyValue("TotalHeads"),
                            TotalSectors = (ulong)disk.GetPropertyValue("TotalSectors"),
                            TotalTracks = (ulong)disk.GetPropertyValue("TotalTracks"),
                            TracksPerCylinder = (uint)disk.GetPropertyValue("TracksPerCylinder")
                        };
                    

                        // Get the logical volumes for the physical disk
                        string assocClass = "Win32_DiskDriveToDiskPartition";
                        string query2 = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{physicalDisk.DeviceID}'}} WHERE AssocClass = {assocClass}";

                        using (var searcher2 = new ManagementObjectSearcher(query2))
                        using (var results2 = searcher2.Get())
                        {
                            foreach (var partition in results2)
                            {
                                string query3 = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";

                                using (var searcher3 = new ManagementObjectSearcher(query3))
                                using (var results3 = searcher3.Get())
                                {
                                    foreach (var volume in results3)
                                    {
                                        // Get the starting offset for the volume
                                        ulong startingOffset = 0;
                                        using (var partitionObj = new ManagementObject($"Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'"))
                                        {
                                            startingOffset = (ulong)partitionObj.GetPropertyValue("StartingOffset");
                                        }
                                        //

                                        var logicalVolume = new LogicalVolume
                                        {
                                            DeviceID = (string)volume.GetPropertyValue("DeviceID"),
                                            VolumeName = (string)volume.GetPropertyValue("VolumeName"),
                                            FileSystem = (string)volume.GetPropertyValue("FileSystem"),
                                            Size = (ulong)volume.GetPropertyValue("Size"),
                                            FreeSpace = (ulong)volume.GetPropertyValue("FreeSpace"),
                                            PercentageUsed = (int)Math.Floor((1 - ((double)(ulong)volume.GetPropertyValue("FreeSpace") / (double)(ulong)volume.GetPropertyValue("Size"))) * 100),
                                            StartingOffset = startingOffset
                                        
                                        };
                                        if(physicalDisk != null)
                                        {
                                            physicalDisk.LogicalVolumes.Add(logicalVolume);
                                        }
                                    }
                                }
                            }
                        }

                        if (physicalDisk != null)
                        {
                            PhysicalDisks.Add(physicalDisk);
                        }
                    }
                }
            }
        }

        private async Task ImageDisk(string sourceDiskPath)
        {
            //// TODO: Open a popup dialogue to get the source and destination disk paths
            
        }
    }
}
