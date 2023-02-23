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

namespace ForensicX.ViewModels
{
    public class DeviceListViewModel : ObservableObject
    {
        public ObservableCollection<Disk> LocalHardDisks { get; }
        public ObservableCollection<Disk> RemovableStorageDisks { get; }

        public DeviceListViewModel()
        {
            LocalHardDisks = new ObservableCollection<Disk>();
            RemovableStorageDisks = new ObservableCollection<Disk>();
            LoadLogicalVolumes();
        }

        private void LoadLogicalVolumes()
        {
            string query = "SELECT * FROM Win32_LogicalDisk";

            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
            {
                int i = 0;
                foreach (var disk in results)
                {
                    string volumeName = (string)disk.GetPropertyValue("VolumeName");
                    var mediaType = disk.GetPropertyValue("MediaType");

                    if (string.IsNullOrEmpty(volumeName))
                    {
                        volumeName = $"Local Drive {LocalHardDisks.Count}"; // If the device has no label, we'll just call it 'Local Drive X'
                    }

                    Disk d = new()
                    {
                        VolumeName = volumeName,
                        Name = (string)disk.GetPropertyValue("Name"),
                        DeviceID = (string)disk.GetPropertyValue("DeviceID"),
                        FileSystem = (string)disk.GetPropertyValue("FileSystem"),
                        Size = (ulong)disk.GetPropertyValue("Size"),
                        FreeSpace = (ulong)disk.GetPropertyValue("FreeSpace"),
                        PercentageUsed = (int)Math.Floor((1 - ((double)(ulong)disk.GetPropertyValue("FreeSpace") / (double)(ulong)disk.GetPropertyValue("Size"))) * 100),
                        Description = (string)disk.GetPropertyValue("Description"),
                        
                    };

                    using (var searcher2 = new ManagementObjectSearcher($"SELECT * FROM MSStorageDriver_ATAPISmartData WHERE InstanceName='\\'\\\\\\{d.DeviceID}\\\\{d.Name}\\\\'\""))
                    using (var results2 = searcher2.Get())
                    {
                        foreach (var driveData in results2)
                        {
                            d.Model = (string)driveData.GetPropertyValue("VendorSpecific");
                            var serialNumber = (byte[])driveData.GetPropertyValue("SerialNumber");
                            d.serialNumber = Encoding.ASCII.GetString(serialNumber);
                        }
                    }

                    LocalHardDisks.Add(d);
                }
            }
        }
    }
}
