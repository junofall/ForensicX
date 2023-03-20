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

        public class FileItem
        {
            public string Name { get; set; }
            public string Size { get; set; }
            public string DateModified { get; set; }
        }

        public HomeView()
        {
            this.InitializeComponent();
            ViewModel = new HomeViewViewModel();
            DataContext = ViewModel;
            ViewModel.DataReady += ViewModel_DataReady;
        }



        private void EvidenceTreeView_ItemInvoked(object sender, TreeViewItemInvokedEventArgs e)
        {
            if (e.InvokedItem is TreeViewNode selectedItem)
            {
                UpdateBreadcrumb(selectedItem);
                // Enable or disable the Up button based on the existence of a parent node
                UpButton.IsEnabled = selectedItem.Parent != null;

                // If the selected node represents a directory, set the ItemsSource of the GridView to its children
                if (selectedItem.Content is FileEntry fileEntry && fileEntry.IsDirectory)
                {
                    ChildrenGridView.ItemsSource = fileEntry.Children;
                }
                else
                {
                    ChildrenGridView.ItemsSource = null;
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

                System.Diagnostics.Debug.WriteLine("Content: " + selectedItem.Content);


                selectedItem = selectedItem.Parent;
            }

            


            Breadcrumb_CurrentItemSelected.ItemsSource = pathItems;
        }

        private void Breadcrumb_CurrentItemSelected_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            // Get the clicked folder
            BreadcrumbBarFolder clickedFolder = args.Item as BreadcrumbBarFolder;

            // Traverse the tree view and select the corresponding item
            SelectItemInTreeView(EvidenceTreeView.RootNodes, clickedFolder);

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
                }
                // Enable or disable the Up button based on the existence of a parent node
                UpButton.IsEnabled = currentNode.Parent != null && currentNode.Parent.Parent != null;
            }
            
        }

        private bool SelectItemInTreeView(IList<TreeViewNode> nodes, BreadcrumbBarFolder targetFolder)
        {
            foreach (TreeViewNode node in nodes)
            {
                if (node.Content.ToString() == targetFolder.Name)
                {
                    // Select the matching item and return true
                    EvidenceTreeView.SelectedNode = node;
                    return true;
                }

                // Recursively search the children nodes
                if (SelectItemInTreeView(node.Children, targetFolder))
                {
                    // Item found in the children, return true
                    return true;
                }
            }

            // Item not found in this level, return false
            return false;
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
                case "TextView":
                    subviewFrame.Navigate(typeof(SubViews.TextView));
                    break;
                case "HexView":
                    subviewFrame.Navigate(typeof(SubViews.HexView));
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

        public void HashCalculator_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
