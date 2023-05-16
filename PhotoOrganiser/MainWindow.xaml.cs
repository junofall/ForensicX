// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ForensicX.ViewModels;
using ForensicX.Views;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Composition.SystemBackdrops;
using System.Runtime.InteropServices;
using WinRT;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml.Documents;
using Windows.System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See below for implementation.
    MicaController m_backdropController;
    SystemBackdropConfiguration m_configurationSource;

    public event EventHandler<double> ProgressUpdated;

    public ProgressBar _imagingProgressBar => StatusBar.FindName("ImagingProgressBar") as ProgressBar;

    public TextBlock _statusBarText => StatusBar.FindName("StatusBarText") as TextBlock;

    public TextBlock _statusBarText2 => StatusBar.FindName("StatusBarText2") as TextBlock;

    public MainWindow()
    {
        this.InitializeComponent();

        TrySetSystemBackdrop();

        Title = "ForensicX";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);
        shellFrame.Navigate(typeof(HomeView));
    }

    private async void OpenPathInExplorer(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        var path = ((Run)sender.Inlines[0]).Text;
        IStorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(path);
        await Launcher.LaunchFolderAsync(storageFolder);
    }

    private void OnProgressUpdated(double progress)
    {
        ProgressUpdated?.Invoke(this, progress);
    }

    private void shellNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            // Navigate to the settings view.
            shellFrame.Navigate(typeof(SettingsView));

            // Update the title.
            Title = "Settings";
        }
        else
        {
            // Get the selected item.
            var selectedItem = (NavigationViewItem)args.SelectedItem;

            // Get the tag of the selected item.
            var tag = selectedItem.Tag?.ToString();

            // Navigate to the corresponding view based on the tag.
            switch (tag)
            {
                case "HomeView":
                    shellFrame.Navigate(typeof(HomeView));
                    Title = "ForensicX | Home";
                    break;
                case "FileDetailsView":
                    shellFrame.Navigate(typeof(EvidenceView));
                    Title = "ForensicX | File Details";
                    break;
                case "DeviceListView":
                    shellFrame.Navigate(typeof(DeviceListView));
                    Title = "ForensicX | Volume List";
                    break;
            }
        }
    }

    bool TrySetSystemBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            // Create the policy object.
            m_configurationSource = new SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;
            ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            m_backdropController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_backdropController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);
            return true; // succeeded
        }

        return false; // Mica is not supported on this system
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed
        // so it doesn't try to use this closed window.
        if (m_backdropController != null)
        {
            m_backdropController.Dispose();
            m_backdropController = null;
        }
        this.Activated -= Window_Activated;
        m_configurationSource = null;
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (m_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme()
    {
        switch (((FrameworkElement)this.Content).ActualTheme)
        {
            case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
            case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
        }
    }
}


class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

    object m_dispatcherQueueController = null;
    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            // one already exists, so we'll just use it.
            return;
        }

        if (m_dispatcherQueueController == null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }
}
