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

namespace ForensicX.ViewModels
{
    public class HomeViewViewModel : ObservableObject
    {
        public ObservableCollection<TreeViewItem> EvidenceTreeItems { get; set; }
        public ObservableCollection<EvidenceItem> EvidenceItems { get; set; }
        public event EventHandler<EvidenceItem> DataReady;

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
        public HomeViewViewModel()
        {
            AddEvidenceCommand = new RelayCommand(AddEvidence);
            EvidenceItems = new ObservableCollection<EvidenceItem>();
        }

        private async void AddEvidence()
        {
            // Create a folder picker
            FileOpenPicker openPicker = new FileOpenPicker();

            var window = (Application.Current as App)?._window as MainWindow;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Create a new EvidenceItem and add it to the EvidenceItems collection
                var evidenceItem = new DiskImageEvidence { Name = file.Name, Path = file.Path };
                evidenceItem.Load();
                evidenceItem.DiskInstance.Initialize();
                evidenceItem.Children = new ObservableCollection<Partition>();
                foreach(Partition p in evidenceItem.DiskInstance.Partitions)
                {
                    evidenceItem.Children.Add(p);
                    System.Diagnostics.Debug.WriteLine("Partition Added");
                };

                window.DispatcherQueue.TryEnqueue(() =>
                {
                    EvidenceItems.Add(evidenceItem);
                    OnDataReady(evidenceItem);
                });
            }
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
                    PrintFileEntries(partition.Children, level + 2);
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

            if (partitions != null)
            {
                foreach (Partition partition in partitions)
                {
                    TreeViewNode partitionNode = new TreeViewNode { Content = partition };
                    foreach (var node in BuildFileEntryNodes(partition.Children))
                    {
                        partitionNode.Children.Add(node);
                    }
                    partitionNode.IsExpanded = false;
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
