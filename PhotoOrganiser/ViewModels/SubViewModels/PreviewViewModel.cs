using CommunityToolkit.Mvvm.ComponentModel;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ForensicX.ViewModels.SubViewModels
{
    public class PreviewViewModel : ObservableObject
    {
        public ImageSource DisplayImage { get; set; }

        private FileEntry? _selectedFile;

        public FileEntry? SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        public async Task SetSelectedFile(FileEntry? value)
        {
            _selectedFile = value;
            OnPropertyChanged(nameof(SelectedFile));

            if (value != null && value.Data != null)
            {
                DisplayImage = await ByteArrayToImageAsync(value.Data);
                OnPropertyChanged(nameof(DisplayImage));
            }
        }

        public ImageSource ByteArrayToImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;

            BitmapImage image = new BitmapImage();
            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            ms.AsStreamForWrite().Write(imageBytes, 0, imageBytes.Length);
            ms.Seek(0);

            image.SetSource(ms);
            ImageSource src = image;

            return src;
        }

        public async Task<ImageSource> ByteArrayToImageAsync(byte[] imageBytes)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
                await ms.AsStreamForWrite().WriteAsync(imageBytes, 0, imageBytes.Length);
                ms.Seek(0);

                await image.SetSourceAsync(ms);
                ImageSource src = image;

                return src;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Unable to preview image. Exception: " + ex.Message);
                // Create a new BitmapImage and set the UriSource to the static error image
                BitmapImage errorImage = new BitmapImage();
                errorImage.UriSource = new Uri("ms-appx:///Assets/Images/ErrorCatW.png");

                return errorImage;
            }
        }
    }
}
