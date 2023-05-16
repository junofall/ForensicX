using Microsoft.UI.Xaml.Data;
using System;

namespace ForensicX.Helpers;

public class FileSizeConverter : IValueConverter
{
    private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ulong sizeInBytes)
        {
            if (sizeInBytes == 0)
            {
                return "0 bytes";
            }

            var suffixIndex = (int)Math.Floor(Math.Log(sizeInBytes, 1024));
            var adjustedSize = Math.Round(sizeInBytes / Math.Pow(1024, suffixIndex), 2);

            return $"{adjustedSize} {SizeSuffixes[suffixIndex]}";
        }

        return "N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}