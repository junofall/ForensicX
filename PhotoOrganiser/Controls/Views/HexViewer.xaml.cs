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
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ForensicX.Controls.Views
{
    public class HexRow
    {
        public string RowNumber { get; set; }
        public string HexData { get; set; }
        public string AsciiData { get; set; }
    }

    public sealed partial class HexViewer : UserControl
    {
        public HexViewer()
        {
            this.InitializeComponent();
        }

        private ObservableCollection<HexRow> GenerateRows(byte[] data)
        {
            ObservableCollection<HexRow> rows = new ObservableCollection<HexRow>();

            for (int i = 0; i < data.Length; i += 16)
            {
                StringBuilder hexData = new StringBuilder();
                StringBuilder asciiData = new StringBuilder();

                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length)
                    {
                        byte b = data[i + j];
                        hexData.AppendFormat("{0:X2} ", b);
                        asciiData.Append(char.IsControl((char)b) ? '.' : (char)b);

                        if (j == 7)
                        {
                            hexData.Append("  ");
                        }
                    }
                    else
                    {
                        // Add padding for incomplete rows
                        hexData.Append("   ");
                        if (j == 7)
                        {
                            hexData.Append("  ");
                        }
                    }
                }

                rows.Add(new HexRow
                {
                    RowNumber = $"{i:X8}",
                    HexData = hexData.ToString(),
                    AsciiData = asciiData.ToString()
                });
            }

            return rows;
        }
    }
}
