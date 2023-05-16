using ForensicX.Controls.Dialogs;
using ForensicX.Helpers;
using ForensicX.Services;
using ForensicX.ViewModels;
using Microsoft.Toolkit.Uwp.Notifications;
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
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    
    public sealed partial class DeviceListView : Page
    {
        private InfoBar adminRightsInfoBar;
        private ContentDialog dialog;
        private ImageDiskDialogContent dialogContent;
        private DeviceListViewModel viewModel;

        public DeviceListView()
        {
            this.InitializeComponent();
            viewModel = (DeviceListViewModel)DataContext;
        }

        private void InfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
        {
            adminRightsInfoBar = null;
        }

        private async void ShowDialog_Click(object sender, RoutedEventArgs e)
        {
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (!isElevated)
            {
                if(adminRightsInfoBar == null)
                {
                    adminRightsInfoBar = new InfoBar
                    {
                        Severity = InfoBarSeverity.Warning,
                        IsOpen = true,
                        IsClosable = true,
                        Title = "Administrative Rights Required",
                        Message = "Physical disk access requires administrative rights. Please restart the application with administrative privileges.",
                    };

                    infoBarContainer.Children.Add(adminRightsInfoBar);
                }
                
                return;
            }

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
                var diskImager = new DiskImager(dialogContent.SourceDevicePath, dialogContent.FullDestinationPath);

                //Toast
                var toastBuilder = new ToastNotifications();
                toastBuilder.SendUpdatableToastWithProgress(0, dialogContent.SourceDevicePath);

                // Subscribe to the ProgressUpdated event
                diskImager.ProgressUpdated += async (s, progress) =>
                {
                    // Use the CoreDispatcher to update the UI on the main thread
                    var window = (Application.Current as App)?._window as MainWindow;

                    // Retrieve the window handle (HWND) of the current WinUI 3 window.
                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                    var fsc = new FileSizeConverter();

                    // Set the progress value of the progress bar.
                    window.DispatcherQueue.TryEnqueue(() =>
                    {
                        // Make the progress bar visible
                        window._imagingProgressBar.Visibility = Visibility.Visible;
                        // Update the progress bar
                        window._imagingProgressBar.Value = progress;

                        // Calculate the total bytes read and stream length
                        var totalBytesRead = diskImager.CurrentTotalBytesRead;
                        var streamLength = diskImager.CurrentStreamLength;

                        // Calculate the progress percentage
                        var progressPercentage = (double)totalBytesRead / streamLength * 100;

                        window._statusBarText.Text = $"Status: Imaging...";
                        // Update the status bar text with the progress percentage and the total bytes copied
                        window._statusBarText2.Text = $"{fsc.Convert(totalBytesRead, null, null, null)} / {fsc.Convert(streamLength, null, null, null)} ({progressPercentage:F2}%)";

                        toastBuilder.UpdateProgress(progress);
                    });

                    // Update the status bar text when the progress is completed.
                    if (progress == 100)
                    {
                        window.DispatcherQueue.TryEnqueue(() =>
                        {
                            window._statusBarText.Text = "Status: Idle";
                            window._imagingProgressBar.Visibility = Visibility.Collapsed;
                            window._statusBarText2.Text = "";
                            window._imagingProgressBar.Value = 0;
                        });

                        // Raise an action completed event here.
                    }
                };

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
    }
}
