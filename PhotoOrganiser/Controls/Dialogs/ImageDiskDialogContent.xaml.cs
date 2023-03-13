using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.AccessCache;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ForensicX.ViewModels;
using ForensicX.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX.Controls.Dialogs
{
    public sealed partial class ImageDiskDialogContent : Page
    {
        public string SelectedFolderPath { get; set; }
        public string SelectedFileName { get; set; }
        public string SourceDevicePath { get; set; }
        public string FullDestinationPath { get; set; }
        public bool IsValidationRequested { get; set; }

        public bool isPathValid;
        public event EventHandler InputChanged;
        public ImageDiskDialogContent(DeviceListViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            OutputFolderPath.Text = "";

            // Create a folder picker
            FolderPicker openPicker = new FolderPicker();

            var window = (Application.Current as App)?._window as MainWindow;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                OutputFolderPath.Text = folder.Path;
                SelectedFileName = FileNameTextBox.Text;
                FullDestinationPath = OutputFolderPath.Text + @"\" + SelectedFileName + ".dd";
                FullOutPath.Text = FullDestinationPath;
            }
            else
            {
                OutputFolderPath.Text = "";
            }

        }

        public void UpdateOutPath(object sender, RoutedEventArgs e)
        {
            SelectedFileName = FileNameTextBox.Text;
            SelectedFolderPath = OutputFolderPath.Text;
            if(SelectedFolderPath.Length > 0 && SelectedFileName.Length > 0)
            {
                FullDestinationPath = OutputFolderPath.Text + @"\" + SelectedFileName + ".dd";
                FullOutPath.Text = FullDestinationPath;
                
                isPathValid = IsValidFilename(SelectedFileName) && IsValidPath(SelectedFolderPath);
            }
            else
            {
                FullOutPath.Text = "";
                isPathValid = false;
            }
            OnInputChanged();
        }

        private void OnInputChanged()
        {
            InputChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool IsValidFilename(string testName)
        {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]");
            if (containsABadCharacter.IsMatch(testName)) { return false; };

            // other checks for UNC, drive-path format, etc

            return true;
        }

        private bool IsValidPath(string path)
        {
            return Path.IsPathFullyQualified(path) && Path.IsPathRooted(path);
        }

        private void ValidateBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateBox.IsChecked = true ? IsValidationRequested = true : IsValidationRequested = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(InputComboBox.SelectedItem != null)
            {
                SourceDevicePath = ((PhysicalDisk)InputComboBox.SelectedItem).DeviceID;
            }
        }

        public void OnDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            // Remove the event handler from the ComboBox
            InputComboBox.SelectionChanged -= ComboBox_SelectionChanged;
        }
    }
}
