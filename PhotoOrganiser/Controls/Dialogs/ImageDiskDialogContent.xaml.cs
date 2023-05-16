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
using Windows.Win32;
using System.Runtime.InteropServices;

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

        private unsafe async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use the CoreDispatcher to update the UI on the main thread
                var window = (Application.Current as App)?._window as MainWindow;

                // Retrieve the window handle (HWND) of the current WinUI 3 window.
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                int hr = PInvoke.CoCreateInstance(
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
                OutputFolderPath.Text = folderPath.ToString();
                SelectedFileName = FileNameTextBox.Text;
                FullDestinationPath = OutputFolderPath.Text + @"\" + SelectedFileName + ".dd";
                FullOutPath.Text = FullDestinationPath;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
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
