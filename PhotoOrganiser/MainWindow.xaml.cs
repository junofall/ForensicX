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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        Title = "ForensicX";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);
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
                    Title = "Home";
                    break;
                case "FileDetailsView":
                    shellFrame.Navigate(typeof(EvidenceView));
                    Title = "File Details";
                    break;
                case "DeviceListView":
                    shellFrame.Navigate(typeof(DeviceListView));
                    Title = "Volume List";
                    break;
            }
        }
    }
}
