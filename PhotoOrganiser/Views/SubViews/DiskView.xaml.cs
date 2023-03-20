using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using ForensicX.Models;
using ForensicX.ViewModels.SubViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX.Views.SubViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DiskView : Page
    {
        public DiskViewSubViewModel ViewModel { get; }

        public DiskView()
        {
            this.InitializeComponent();
            ViewModel = new DiskViewSubViewModel();
            DataContext = ViewModel;
        }
    }

    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isAllocated)
            {
                return isAllocated
                    ? new SolidColorBrush(Colors.ForestGreen)
                    : new SolidColorBrush(Colors.Transparent);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
