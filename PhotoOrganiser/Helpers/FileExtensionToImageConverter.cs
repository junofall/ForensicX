using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ForensicX.Helpers
{
    public class FileExtensionToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string fileExtension))
            {
                Debug.WriteLine("FileExtensionToImageConverter: value is not a string");
                return new BitmapImage(new Uri("ms-appx:///Assets/Icons/default.png"));
            }

            Debug.WriteLine("FileExtensionToImageConverter: Extension is " + fileExtension.ToLower());
            fileExtension = fileExtension.ToLower();
            switch (fileExtension)
            {
                case "7z":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/zip.png"));
                case "zip":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/zip.png"));
                case "rar":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/zip.png"));
                case "gz":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/zip.png"));
                case "jpg":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/image.png"));
                case "png":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/image.png"));
                case "3fr":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/3fr.png"));
                case "docx":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/docx.png"));
                case "css":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/css.png"));
                case "html":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/html.png"));
                case "gif":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/image.png"));
                case "pdf":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/pdf.png"));
                case "exe":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/exe.png"));
                case "avi":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/video.png"));
                case "mov":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/video.png"));
                case "txt":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/txt.png"));
                case "mp4":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/video.png"));
                case "wav":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/audio.png"));
                case "mp3":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/audio.png"));
                default:
                    return new BitmapImage(new Uri("ms-appx:///Assets/Icons/default.png"));
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
