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

namespace ForensicX.ViewModels
{
    public class DeviceListViewModel : ObservableObject
    {
        public ObservableCollection<string> LocalHardDisks { get; }
        public ObservableCollection<string> RemovableStorageDisks { get; }

        public DeviceListViewModel()
        {
            LocalHardDisks = new ObservableCollection<string>();
            RemovableStorageDisks = new ObservableCollection<string>();

            LoadLogicalVolumes();
        }

        private void LoadLogicalVolumes()
        {
            string query_lhd = "SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"; // 3 = local hard disks
            string query_rm = "SELECT * FROM Win32_LogicalDisk WHERE DriveType=2"; // 2 = removable media

            using (var searcher = new ManagementObjectSearcher(query_lhd))
            using (var results = searcher.Get())
            {
                foreach (var disk in results)
                {
                    string volumeName = (string)disk.GetPropertyValue("VolumeName");

                    if (string.IsNullOrEmpty(volumeName))
                    {
                        volumeName = $"Local Drive {LocalHardDisks.Count}"; // If the device has no label, we'll just call it 'Local Drive X'
                    }
                    
                    LocalHardDisks.Add(volumeName + " (" + (string)disk.GetPropertyValue("Name") + ")"); // Append the logical drive letter to the volume name
                }
            }


            using (var searcher = new ManagementObjectSearcher(query_rm))
            using (var results = searcher.Get())
            {
                foreach (var disk in results)
                {
                    string volumeName = (string)disk.GetPropertyValue("VolumeName");

                    if (string.IsNullOrEmpty(volumeName))
                    {
                        volumeName = $"Removable Storage Device {LocalHardDisks.Count}";
                    }

                    RemovableStorageDisks.Add(volumeName + " (" + (string)disk.GetPropertyValue("Name") + ")");
                }
            }

            if(RemovableStorageDisks.Count == 0)
            {
                RemovableStorageDisks.Add("No removable media connected.");
            }
        }
    }
}
