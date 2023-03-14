using ForensicX.Controls.Dialogs;
using ForensicX.Services;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    
    public sealed partial class DeviceListView : Page
    {
        private ContentDialog dialog;
        private ImageDiskDialogContent dialogContent;
        private DeviceListViewModel viewModel;

        public DeviceListView()
        {
            this.InitializeComponent();
            viewModel = (DeviceListViewModel)DataContext;
            Debug.WriteLine("Process Elevated? " + isElevated());
        }

        private async void ShowDialog_Click(object sender, RoutedEventArgs e)
        {
            dialog = new ContentDialog();

            dialogContent = new ImageDiskDialogContent(viewModel);

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Create Disk Image";
            dialog.PrimaryButtonText = "Image";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.IsPrimaryButtonEnabled = false;
            dialog.Content = dialogContent;

            // Attach the event handler to the event
            dialogContent.InputChanged += OnImageDiskDialogInputChanged;

            var result = await dialog.ShowAsync();

            dialog.Closed += dialogContent.OnDialogClosed;

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine($"User initiated imaging! Source: {dialogContent.SourceDevicePath} ; Destination: {dialogContent.FullDestinationPath}");

                DiskImager diskImager = new DiskImager(@dialogContent.SourceDevicePath, @dialogContent.FullDestinationPath);
                await diskImager.Start();
            }
            else
            {
                Debug.WriteLine("User cancelled imaging!");
            }
            
        }

        private void OnImageDiskDialogInputChanged(object sender, EventArgs e)
        {
            // Disable the primary button if the input fields are not valid
            dialog.IsPrimaryButtonEnabled = dialogContent.isPathValid;
            Debug.WriteLine("Is Path Valid? : " + dialogContent.isPathValid);
        }

        private bool isElevated()
        {
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
