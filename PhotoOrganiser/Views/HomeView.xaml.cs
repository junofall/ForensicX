using ForensicX.Models;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using ForensicX.Models.Disks;
using ForensicX.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ForensicX.Models.Evidence;
using Windows.Storage.Streams;
using ForensicX.Interfaces;
using System.Diagnostics;
using System.Collections;
using System.Text;
using ForensicX.ViewModels.SubViewModels;
using ForensicX.Views.SubViews;
using ForensicX.Controls.Views;
using ForensicX.Controls.Dialogs;
using ForensicX.Helpers;
using ForensicX.Services;
using Windows.Storage.Pickers;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeView : Page
    {
        public HomeViewViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.DataReady += ViewModel_DataReady;
            // Set up any other necessary event subscriptions or bindings here
            RebuildTreeView();

        }

        private void RebuildTreeView()
        {
            foreach (EvidenceItem eItem in ViewModel.EvidenceItems)
            {
                InitializeTreeView(eItem);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.DataReady -= ViewModel_DataReady;
            // Clean up any other event subscriptions or bindings here
        }

        public class FileItem
        {
            public string Name { get; set; }
            public string Size { get; set; }
            public string DateModified { get; set; }
        }

        public HomeView()
        {
            this.InitializeComponent();
            ViewModel = ((ViewModelLocator)Application.Current.Resources["ViewModelLocator"]).HomeViewViewModel;
            DataContext = ViewModel;
            ViewModel.IsAddingEvidenceItemChanged += ViewModel_IsAddingEvidenceItemChanged;
        }

        private void ViewModel_IsAddingEvidenceItemChanged(object sender, bool isAdding)
        {
            EvidenceItemAddingBox.Visibility = isAdding ? Visibility.Visible : Visibility.Collapsed;
        }


        private void EvidenceTreeView_ItemInvoked(object sender, TreeViewItemInvokedEventArgs e)
        {
            if (e.InvokedItem is TreeViewNode selectedItem)
            {
                UpdateBreadcrumb(selectedItem);
                UpdateChildrenGridView(selectedItem);
                // Enable or disable the Up button based on the existence of a parent node
                UpButton.IsEnabled = selectedItem.Parent != null;
                RemoveButton.IsEnabled = selectedItem.Parent != null;
            }
            else
            {
                Debug.WriteLine("EvidenceTreeView_ItemInvoked was of unknown type " + e.GetType());
            }
        }

        private async void ChildrenGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is FileEntry fileEntry && !fileEntry.IsDirectory)
                {
                    Debug.WriteLine("ChildrenGridView_ItemClick: fileEntry.Name is '" + fileEntry.Name + "'");
                    FileSystem fileSystem = fileEntry.FileSystem;
                    await fileSystem.LoadFileEntryDataAsync(fileEntry);

                    Debug.WriteLine("LoadFileEntryDataAsync passed over. Done??");

                    var hexView = this.subviewFrame.Content as HexView;

                    // If the current content is HexView and its DataContext is HexViewModel, set the file
                    if (hexView != null && hexView.DataContext is HexViewModel hexViewModel)
                    {
                        Debug.WriteLine("Setting file for the hexview model...");
                        await hexViewModel.SetSelectedFile(fileEntry);
                    }
                    else
                    {
                        var previewView = this.subviewFrame.Content as FileView;
                        if (previewView != null && previewView.DataContext is PreviewViewModel previewViewModel)
                        {
                            Debug.WriteLine("Set selected file for PreviewViewmodel");
                            await previewViewModel.SetSelectedFile(fileEntry);
                        }
                        else
                        {
                            var infoView = this.subviewFrame.Content as InfoView;
                            if (infoView != null && infoView.DataContext is InfoViewModel infoViewModel)
                            {
                                Debug.WriteLine("Set selected file for Info View Model");
                                infoViewModel.SelectedFile = fileEntry;
                            }
                        }

                        Debug.WriteLine("Either subviewFrame.Content is not HexView or HexView.DataContext is not HexViewModel");
                        if (hexView != null)
                            Debug.WriteLine("HexView.DataContext is: " + hexView.DataContext.GetType());
                    }
                }
                else if(e.ClickedItem is FileEntry dirEntry && dirEntry.IsDirectory)
                {
                    Debug.WriteLine("Directory " + dirEntry.Name + " Clicked");
                }
                else
                {
                    Debug.WriteLine(e.ClickedItem.GetType());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception from HomeView.xaml.cs ChildGridView_ItemClick");
                Debug.WriteLine(ex.Message);
            }
        }


        private unsafe void ChildrenGridView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).Properties.IsRightButtonPressed)
            {
                var itemsControl = (ItemsControl)sender;
                var result = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(null).Position, itemsControl);
                var clickedItemContainer = result.OfType<GridViewItem>().FirstOrDefault();

                if (clickedItemContainer != null)
                {
                    FileEntry fileEntry = (FileEntry)clickedItemContainer.Content;

                    // Create the context menu
                    var contextMenu = new MenuFlyout();

                    if (fileEntry.IsDirectory)
                    {
                        // Add context menu options specific to directories
                        var dirOpenMenuItem = new MenuFlyoutItem { Text = "Open" };
                        dirOpenMenuItem.Click += (s, args) =>
                        {
                            // Handle the directory opening logic here
                        };
                        contextMenu.Items.Add(dirOpenMenuItem);

                        contextMenu.Items.Add(new MenuFlyoutSeparator());

                        // Add context menu options specific to directories
                        var dirExtractMenuItem = new MenuFlyoutItem { Text = "Extract Directory..." };
                        dirExtractMenuItem.Click += (s, args) =>
                        {
                            // Handle the directory opening logic here
                        };
                        contextMenu.Items.Add(dirExtractMenuItem);
                    }
                    else
                    {
                        // Add context menu options specific to files
                        var extractMenuItem = new MenuFlyoutItem { Text = "Extract File..." };
                        extractMenuItem.Click += async (s, args) =>
                        {
                            // Use the CoreDispatcher to update the UI on the main thread
                            var window = (Application.Current as App)?._window as MainWindow;

                            // Retrieve the window handle (HWND) of the current WinUI 3 window.
                            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

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

                            // Set the dialog to be a folder picker.
                            fod.SetOptions(Windows.Win32.UI.Shell.FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS);

                            // Show the dialog.
                            fod.Show(new Windows.Win32.Foundation.HWND(hWnd));

                            // Get the selected folder.
                            fod.GetResult(out var ppsi);

                            Windows.Win32.Foundation.PWSTR folderPath;
                            ppsi.GetDisplayName(Windows.Win32.UI.Shell.SIGDN.SIGDN_FILESYSPATH, &folderPath);

                            // Set the folder path.
                            var OutputFolderPath = folderPath.ToString();
                            
                            var FullDestinationPath = OutputFolderPath + @"\";

                            fileEntry.FileSystem.ExtractFile(fileEntry, FullDestinationPath);
                        };

                        contextMenu.Items.Add(extractMenuItem);
                    }

                    // Add any common context menu options for both files and directories here
                    // ...

                    // Show the context menu
                    contextMenu.ShowAt(itemsControl, e.GetCurrentPoint(itemsControl).Position);
                }
            }
        }

        private void UpdateBreadcrumb(TreeViewNode selectedItem)
        {
            var pathItems = new ObservableCollection<BreadcrumbBarFolder>();

            while (selectedItem.Content != null)
            {
                if (selectedItem.Content is FileEntry fileEntry)
                {
                    pathItems.Insert(0, new BreadcrumbBarFolder { Name = fileEntry.Name });
                    System.Diagnostics.Debug.WriteLine("Content: " + fileEntry.Name);
                }
                else if (selectedItem.Content is Partition partitionEntry)
                {
                    pathItems.Insert(0, new BreadcrumbBarFolder { Name = partitionEntry.Name });
                    System.Diagnostics.Debug.WriteLine("Content: " + partitionEntry.Name);
                }
                else if (selectedItem.Content is DiskImageEvidence diskImageEvidence)
                {
                    pathItems.Insert(0, new BreadcrumbBarFolder { Name = diskImageEvidence.Name });
                    System.Diagnostics.Debug.WriteLine("Content: " + diskImageEvidence.Name);
                }
                else if (selectedItem.Content is Volume volume)
                {
                    pathItems.Insert(0, new BreadcrumbBarFolder { Name = volume.Name });
                    System.Diagnostics.Debug.WriteLine("Content: " + volume.Name);
                }
                else if (selectedItem.Content.ToString() == "[rootdir]")
                {
                    pathItems.Insert(0, new BreadcrumbBarFolder { Name = "[rootdir]" });
                    System.Diagnostics.Debug.WriteLine("Content: [rootdir]");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("BreadcrumbBar - Unhandled Content: " + selectedItem.Content);
                }

                selectedItem = selectedItem.Parent;
            }
            Breadcrumb_CurrentItemSelected.ItemsSource = pathItems;
        }

        private void Breadcrumb_CurrentItemSelected_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            // Get the clicked folder
            BreadcrumbBarFolder clickedFolder = args.Item as BreadcrumbBarFolder;

            // Get the target partition name
            string targetPartitionName = null;
            if (args.Index > 0)
            {
                targetPartitionName = (Breadcrumb_CurrentItemSelected.ItemsSource as ObservableCollection<BreadcrumbBarFolder>)[args.Index - 1].Name;
            }

            // Traverse the tree view and select the corresponding item within the target partition
            SelectItemInTreeView(EvidenceTreeView.RootNodes, clickedFolder, targetPartitionName);

            // Update the breadcrumb bar
            var items = Breadcrumb_CurrentItemSelected.ItemsSource as ObservableCollection<BreadcrumbBarFolder>;
            for (int i = items.Count - 1; i >= args.Index + 1; i--)
            {
                items.RemoveAt(i);
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            var currentNode = EvidenceTreeView.SelectedNode;
            if (currentNode != null)
            {
                var parentNode = currentNode.Parent;
                if (parentNode != null)
                {
                    EvidenceTreeView.SelectedNode = parentNode;
                    UpdateBreadcrumb(parentNode);
                    UpdateChildrenGridView(parentNode);
                }
                // Enable or disable the Up button based on the existence of a parent node
                UpButton.IsEnabled = currentNode.Parent != null && currentNode.Parent.Parent != null;
            }
            
        }

        private void UpdateChildrenGridView(TreeViewNode selectedItem)
        {
            Debug.WriteLine("UpdateChildrenGridView method invoked.");
            if (selectedItem.Content is FileEntry fileEntry && fileEntry.IsDirectory)
            {
                ChildrenGridView.Visibility = Visibility.Visible;
                ChildrenGridView.ItemsSource = fileEntry.Children;
                EvidenceDashboard.Visibility = Visibility.Collapsed;
            }
            else if (selectedItem.Content is EvidenceItem eItem)
            {
                EvidenceDashboard.Visibility = Visibility.Visible;
                EvidenceDashboard.EvidenceItem = eItem;
                ViewModel.UpdateSelectedEvidenceItem(eItem);
                ChildrenGridView.Visibility = Visibility.Collapsed;
                ChildrenGridView.ItemsSource = null;
            }
            else if (selectedItem.Content is Partition partition)
            {
                Debug.WriteLine("UpdateChildrenGridView: Partition selected");
                EvidenceDashboard.Visibility = Visibility.Collapsed;
                ChildrenGridView.Visibility = Visibility.Visible;
            }
            else if (selectedItem.Content is Volume volume)
            {
                Debug.WriteLine("UpdateChildrenGridView: Volume selected");
                EvidenceDashboard.Visibility = Visibility.Collapsed;
                ChildrenGridView.Visibility = Visibility.Visible;
            }
            else if (selectedItem.Content.ToString() == "[rootdir]")
            {
                if (selectedItem.Parent.Parent != null && selectedItem.Parent.Parent.Content is Partition partition1)
                {
                    EvidenceDashboard.Visibility = Visibility.Collapsed;
                    ChildrenGridView.Visibility = Visibility.Visible;
                    ChildrenGridView.ItemsSource = partition1.Volume.Children;
                }
            }
            else
            {
                Debug.WriteLine("UpdateChildrenGridView TreeViewNode content was of unknown type " + selectedItem.Content.GetType());
                ChildrenGridView.ItemsSource = null;
            }
        }

        private bool SelectItemInTreeView(IList<TreeViewNode> nodes, BreadcrumbBarFolder targetFolder, string targetPartitionName = null)
        {
            try
            {
                foreach (TreeViewNode node in nodes)
                {
                    bool isMatchingNode = node.Content.ToString() == targetFolder.Name;
                    if (targetPartitionName != null && node.Parent != null && node.Parent.Parent.Content != null)
                    {
                        isMatchingNode &= node.Parent.Parent.Content.ToString() == targetPartitionName;
                    }

                    if (isMatchingNode)
                    {
                        // Select the matching item and return true
                        EvidenceTreeView.SelectedNode = node;

                        // Update the ChildrenGridView
                        UpdateChildrenGridView(node);

                        return true;
                    }

                    // Recursively search the children nodes
                    if (SelectItemInTreeView(node.Children, targetFolder, targetPartitionName))
                    {
                        // Item found in the children, return true
                        return true;
                    }
                }

                // Item not found in this level, return false
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in breadcrumb:" + e.Message);
                return false;
            }
        }

        private void ViewModel_DataReady(object sender, EvidenceItem item)
        {
            InitializeTreeView(item);
        }

        private void InitializeTreeView(EvidenceItem item)
        {
            EvidenceTreeView.RootNodes.Add(HomeViewViewModel.BuildTreeView(item));
        }

        public void SubviewNavigated_SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e) 
        {
            // Get the selected item.
            var selectedItem = (NavigationViewItem)e.SelectedItem;

            // Get the tag of the selected item.
            var tag = selectedItem.Tag?.ToString();

            // Navigate to the corresponding view based on the tag.
            switch (tag)
            {
                case "InfoView":
                    subviewFrame.Navigate(typeof(SubViews.InfoView));
                    break;
                case "TextView":
                    subviewFrame.Navigate(typeof(SubViews.TextView));
                    break;
                case "HexView":
                    subviewFrame.Navigate(typeof(SubViews.HexView));
                    Debug.WriteLine("Navigated to HexView");
                    break;
                case "DiskView":
                    subviewFrame.Navigate(typeof(SubViews.DiskView));
                    break;
                case "FileView":
                    subviewFrame.Navigate(typeof(SubViews.FileView));
                    break;
            }
        }

        public void Timeline_Click(object sender, RoutedEventArgs e)
        {

        }

        public void OpenCase_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Logs_Click(object sender, RoutedEventArgs e)
        {

        }

        public void ConvertToVhd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConvertToVhd();
        }

        public void FileSignatureAnalyzer_Click(object sender, RoutedEventArgs e)
        {

        }

        public void EvidenceList_Click(object sender, RoutedEventArgs e)
        {

        }

        public void EditCaseMetadata_Click(object sender, RoutedEventArgs e)
        {

        }

        public void DataRecovery_Click(object sender, RoutedEventArgs e)
        {

        }
        public void CreateCase_Click(object sender, RoutedEventArgs e)
        {

        }
        public void CaseOverview_Click(object sender, RoutedEventArgs e)
        {

        }
        public void AddEvidenceItem_Click(object sender, RoutedEventArgs e)
        {

        }

        public void FullExtraction_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
