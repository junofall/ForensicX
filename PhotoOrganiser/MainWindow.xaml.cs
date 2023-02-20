// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PhotoOrganiser.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PhotoOrganiser;
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

        ViewModel = Ioc.Default.GetService<MainWindowViewModel>();
    }

    public MainWindowViewModel? ViewModel { get; }
    public StorageFolder? SelectedInputFolder { get; set; }
    public StorageFolder? SelectedOutputFolder { get; set; }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        ContentDialogResult result = await StartSettingsDialog.ShowAsync();
        if(result is ContentDialogResult.Primary)
        {

        }
    }

    private async Task<StorageFolder?> SelectFolderAsync()
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        return await folderPicker.PickSingleFolderAsync();
    }

    private async void SelectInputFolderButton_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder? folder = await SelectFolderAsync();
        if(folder != null && ViewModel != null)
        {
            SelectedInputFolder = folder;
            SelectedInputFolderTextBox.Text = SelectedInputFolder.Path;
        }
    }

    private async void SelectOutputFolderButton_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder? folder = await SelectFolderAsync();
        if (folder != null && ViewModel != null)
        {
            SelectedOutputFolder = folder;
            SelectedOutputFolderTextBox.Text = SelectedOutputFolder.Path;
        }
    }

    private void FolderStructureCheckBox_Click(object sender, RoutedEventArgs e) => UpdateOutputFolderExample();

    private void UpdateOutputFolderExample()
    {
        string example = @"[Output]";

        if(SelectedOutputFolder?.Path.Length > 0)
        {
            example = SelectedOutputFolder.Path;
        }

        example += DateTime.Now.ToString(CreateDateFolderFormat(), CultureInfo.InvariantCulture);
        example += @"\[Filename]";

        ExampleTextBlock.Text = example;
    }

    private string CreateDateFolderFormat()
    {
        string format = string.Empty;

        if(CreateYearFolderCheckbox.IsChecked == true)
        {
            format += @"\\yyyy";
        }
        if(CreateMonthFolderCheckbox.IsChecked == true) 
        {
            format += @"\\MM";
        }
        if(CreateDayFolderCheckbox.IsChecked == true)
        {
            format += @"\\dd";
        }
        if(CreateDateFolderCheckbox.IsChecked == true)
        {
            format += @"\\yyyy-MM-dd";
        }

        return format;
    }
}
