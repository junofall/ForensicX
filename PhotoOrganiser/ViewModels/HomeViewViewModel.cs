using CommunityToolkit.Mvvm.Input;
using ForensicX.Models;
using ForensicX.Models.Evidence;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Microsoft.UI.Xaml;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using ForensicX.Models.Disks;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using System.IO;
using Windows.Win32.UI.Shell.Common;
using ForensicX.Helpers;

namespace ForensicX.ViewModels
{
    public class HomeViewViewModel : ObservableObject
    {
        public ObservableCollection<TreeViewItem> EvidenceTreeItems { get; set; }
        public ObservableCollection<EvidenceItem> EvidenceItems { get; set; }
        public event EventHandler<EvidenceItem> DataReady;
        public event EventHandler<bool> IsAddingEvidenceItemChanged;
        private FileSizeConverter fsc = new();

        protected void OnIsAddingEvidenceItemChanged(bool isAdding)
        {
            IsAddingEvidenceItemChanged?.Invoke(this, isAdding);
        }


        private EvidenceItem _selectedEvidenceItem;

        public EvidenceItem SelectedEvidenceItem
        {
            get => _selectedEvidenceItem;
            set
            {
                _selectedEvidenceItem = value;
                // NotifyPropertyChanged if you are implementing INotifyPropertyChanged
            }
        }

        public ICommand AddEvidenceCommand { get; }
        public ICommand RemoveEvidenceCommand { get; }
        public ICommand ExtractFileCommand { get; }

        public HomeViewViewModel()
        {
            AddEvidenceCommand = new RelayCommand(AddEvidence);
            EvidenceItems = new ObservableCollection<EvidenceItem>();
            RemoveEvidenceCommand = new RelayCommand(RemoveSelectedEvidenceItem);
        }

        private void RemoveSelectedEvidenceItem()
        {
            if (SelectedEvidenceItem != null)
            {
                EvidenceItems.Remove(SelectedEvidenceItem);
                System.Diagnostics.Debug.WriteLine("Item Removed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Cannot remove, item is null.");
            }
        }

        public async void ConvertToVhd()
        {
            try
            {
                var window = (Application.Current as App)?._window as MainWindow;

                // Retrieve the window handle (HWND) of the current WinUI 3 window.
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                string selectedFilePath = await PickFileAsync(hWnd);

                if (!string.IsNullOrEmpty(selectedFilePath))
                {
                    Debug.WriteLine("Converting to fixed VHD.");
                    VhdConverter.ConvertToFixedVhd(selectedFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }
        }

        private async void AddEvidence()
        {
            try {
                var window = (Application.Current as App)?._window as MainWindow;

                // Retrieve the window handle (HWND) of the current WinUI 3 window.
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                string selectedFilePath = await PickFileAsync(hWnd);

                if (!string.IsNullOrEmpty(selectedFilePath))
                {
                    OnIsAddingEvidenceItemChanged(true);

                    // Create a new EvidenceItem and add it to the EvidenceItems collection
                    var evidenceItem = new DiskImageEvidence { Name = Path.GetFileName(selectedFilePath), Path = selectedFilePath };
                    await evidenceItem.LoadAsync();
                    await evidenceItem.DiskInstance.InitializeAsync();
                    evidenceItem.Children = new ObservableCollection<Partition>();
                    foreach (Partition p in evidenceItem.DiskInstance.Partitions)
                    {
                        evidenceItem.Children.Add(p);
                        Debug.WriteLine("Partition Added");
                    };

                    window.DispatcherQueue.TryEnqueue(() =>
                    {
                        EvidenceItems.Add(evidenceItem);
                        OnDataReady(evidenceItem);
                        OnIsAddingEvidenceItemChanged(false);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }
        }


        private async void ExtractFile(FileEntry fileEntry)
        {
            try
            {
                var window = (Application.Current as App)?._window as MainWindow;

                // Retrieve the window handle (HWND) of the current WinUI 3 window.
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                string selectedFilePath = await PickFileAsync(hWnd);

                if (!string.IsNullOrEmpty(selectedFilePath))
                {
                    OnIsAddingEvidenceItemChanged(true);

                    // Create a new EvidenceItem and add it to the EvidenceItems collection
                    var evidenceItem = new DiskImageEvidence { Name = Path.GetFileName(selectedFilePath), Path = selectedFilePath };
                    await evidenceItem.LoadAsync();
                    await evidenceItem.DiskInstance.InitializeAsync();
                    evidenceItem.Children = new ObservableCollection<Partition>();
                    foreach (Partition p in evidenceItem.DiskInstance.Partitions)
                    {
                        evidenceItem.Children.Add(p);
                        Debug.WriteLine("Partition Added");
                    };

                    window.DispatcherQueue.TryEnqueue(() =>
                    {
                        EvidenceItems.Add(evidenceItem);
                        OnDataReady(evidenceItem);
                        OnIsAddingEvidenceItemChanged(false);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }
        }


        private async Task<string> PickFileAsync(IntPtr hWnd)
        {
            return await Task.Run(() =>
            {
                string filePath = "";
                try
                {
                    int hr = Windows.Win32.PInvoke.CoCreateInstance(
                    typeof(Windows.Win32.UI.Shell.FileOpenDialog).GUID,
                    null,
                    Windows.Win32.System.Com.CLSCTX.CLSCTX_INPROC_SERVER,
                    typeof(Windows.Win32.UI.Shell.IFileOpenDialog).GUID,
                    out var obj);
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    Windows.Win32.UI.Shell.IFileOpenDialog fod = (Windows.Win32.UI.Shell.IFileOpenDialog)obj;

                    // Show the dialog.
                    fod.Show(new Windows.Win32.Foundation.HWND(hWnd));

                    // Get the selected file.
                    fod.GetResult(out var ppsi);

                    unsafe
                    {
                        Windows.Win32.Foundation.PWSTR selectedFilePathPtr;
                        ppsi.GetDisplayName(Windows.Win32.UI.Shell.SIGDN.SIGDN_FILESYSPATH, &selectedFilePathPtr);
                        filePath = selectedFilePathPtr.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }
                return filePath;
            });
        }

        public void UpdateSelectedEvidenceItem(EvidenceItem selectedItem)
        {
            SelectedEvidenceItem = selectedItem;
        }

        private void OnDataReady(EvidenceItem item)
        {
            DataReady?.Invoke(this, item);
        }

        public static void PrintStructure(EvidenceItem item, int level = 0)
        {
            // Print the current EvidenceItem
            string indent = new string(' ', level * 4);
            System.Diagnostics.Debug.WriteLine($"{indent}Evidence Item: {item.Name}");

            // Print the partitions
            if (item.Children != null)
            {
                foreach (Partition partition in item.Children)
                {
                    System.Diagnostics.Debug.WriteLine($"{indent}    Partition: {partition.Name}");
                    PrintFileEntries(partition.Volume.Children, level + 2);
                }
            }
        }

        public static void PrintFileEntries(List<FileEntry> entries, int level = 0)
        {
            if (entries == null) return;

            string indent = new string(' ', level * 4);
            foreach (FileEntry entry in entries)
            {
                System.Diagnostics.Debug.WriteLine($"{indent}{(entry.IsDirectory ? "Directory" : "File")}: {entry.Name}");
                if (entry.IsDirectory && entry.Children != null)
                {
                    PrintFileEntries(entry.Children, level + 1);
                }
            }
        }





        public static TreeViewNode BuildTreeView(EvidenceItem item)
        {
            TreeViewNode evidenceNode = new TreeViewNode { Content = item };
            foreach (var node in BuildPartitionNodes(item.Children))
            {
                evidenceNode.Children.Add(node);
            }
            evidenceNode.IsExpanded = false;
            return evidenceNode;
        }

        public static List<TreeViewNode> BuildPartitionNodes(ObservableCollection<Partition> partitions)
        {
            List<TreeViewNode> partitionNodes = new List<TreeViewNode>();
            FileSizeConverter fsc = new FileSizeConverter();
            if (partitions != null)
            {
                foreach (Partition partition in partitions)
                {
                    // Create a partition node with the size information
                    TreeViewNode partitionNode = new TreeViewNode
                    {
                        Content = partition
                    };

                    if (partition.Volume != null)
                    {
                        // Create a volume node with the volume name and file system type
                        TreeViewNode volumeNode = new TreeViewNode
                        {
                            Content = partition.Volume
                        };

                        // Create a separate TreeViewNode for the root directory
                        TreeViewNode rootDirectoryNode = new TreeViewNode { Content = "[rootdir]" };

                        volumeNode.Children.Add(rootDirectoryNode);

                        // Build the FileEntry nodes for the volume
                        foreach (var node in BuildFileEntryNodes(partition.Volume.Children))
                        {
                            rootDirectoryNode.Children.Add(node);
                        }

                        // Add the volume node to the partition node
                        partitionNode.Children.Add(volumeNode);
                        partitionNode.IsExpanded = false;
                    }

                    partitionNodes.Add(partitionNode);
                }
            }

            return partitionNodes;
        }

        public static List<TreeViewNode> BuildFileEntryNodes(List<FileEntry> entries)
        {
            List<TreeViewNode> fileEntryNodes = new List<TreeViewNode>();

            if (entries == null) return fileEntryNodes;

            foreach (FileEntry entry in entries)
            {
                // Only add directories to the TreeView
                if (entry.IsDirectory)
                {
                    TreeViewNode fileEntryNode = new TreeViewNode { Content = entry };

                    if (entry.Children != null)
                    {
                        foreach (var node in BuildFileEntryNodes(entry.Children))
                        {
                            fileEntryNode.Children.Add(node);
                        }
                    }

                    fileEntryNode.IsExpanded = false;
                    fileEntryNodes.Add(fileEntryNode);
                }
            }

            return fileEntryNodes;
        }
    }
}
