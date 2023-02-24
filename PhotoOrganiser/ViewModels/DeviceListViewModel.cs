using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Management;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Converters;
using ForensicX.Models;
using ForensicX.Helpers;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ForensicX.Services;

namespace ForensicX.ViewModels
{
    public class DeviceListViewModel : ObservableObject
    {
        private readonly DeviceWatcherService _deviceWatcherService;
        public ObservableCollection<PhysicalDisk> PhysicalDisks { get; }
        public ICommand LoadDisksCommand { get; }

        public DeviceListViewModel()
        {
            PhysicalDisks = new ObservableCollection<PhysicalDisk>();
            LoadDisksCommand = new RelayCommand(LoadDisks);
            LoadDisks();

            _deviceWatcherService = new DeviceWatcherService();
            _deviceWatcherService.DeviceChanged += DeviceWatcher_OnDeviceChanged;
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
                    var physicalDisk = new PhysicalDisk
                    {
                        DeviceID = (string)disk.GetPropertyValue("DeviceID"),
                        Model = (string)disk.GetPropertyValue("Model"),
                        SerialNumber = (string)disk.GetPropertyValue("SerialNumber"),
                        Size = (ulong)disk.GetPropertyValue("Size"),
                        LogicalVolumes = new List<LogicalVolume>(),
                        MediaType = (string)disk.GetPropertyValue("MediaType")
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
                                    physicalDisk.LogicalVolumes.Add(logicalVolume);
                                }
                            }
                        }
                    }
                    PhysicalDisks.Add(physicalDisk);
                }
            }
        }

        private void DeviceWatcher_OnDeviceChanged(object sender, EventArgs e)
        {
            // Handle device change event here
            // Reload disks or do other necessary actions
            LoadDisks();
        }

        public void DeviceWatcher_Dispose()
        {
            _deviceWatcherService.DeviceChanged -= DeviceWatcher_OnDeviceChanged;
            _deviceWatcherService.Stop();
        }
    }
}
