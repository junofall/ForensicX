using CommunityToolkit.Mvvm.ComponentModel;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using ForensicX.Models.Formatting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.ViewModels.SubViewModels
{
    public class HexViewModel : ObservableObject
    {
        private string? _hexData;
        private string? _asciiData;

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

        public string? AsciiData
        {
            get => _asciiData;
            set => SetProperty(ref _asciiData, value);
        }

        public string? HexData
        {
            get => _hexData;
            set => SetProperty(ref _hexData, value);
        }

        public async Task SetSelectedFile(FileEntry fileEntry)
        {
            if (fileEntry == null)
            {
                throw new ArgumentNullException(nameof(fileEntry));
            }

            Debug.WriteLine("SetSelectedFile @ HexViewModel - fileEntry.Name is '" + fileEntry.Name + "'");

            while (fileEntry?.Data == null)
            {
                Debug.WriteLine("SelectedFile.Data is null, waiting...");
                await Task.Delay(1000); // Wait for 100ms before checking again
            }

            SelectedFile = fileEntry; // This sets the property and fires the PropertyChanged event synchronously
            await PrintByteArrayAsync(SelectedFile?.Data);  // Now we can call the async method
            
        }

        public async Task PrintByteArrayAsync(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder();
            StringBuilder ascii = new StringBuilder();

            await Task.Run(() =>
            {
                Debug.WriteLine("Task Run PrintByte Len: " + byteArray.Length);
                for (int i = 0; i < byteArray.Length; i++)
                {
                    hex.Append(byteArray[i].ToString("X2") + " ");
                    if (byteArray[i] >= 32 && byteArray[i] <= 126)
                    {
                        ascii.Append((char)byteArray[i]);
                    }
                    else
                    {
                        ascii.Append(".");
                    }

                    if ((i + 1) % 16 == 0)
                    {
                        hex.Append(Environment.NewLine);
                        ascii.Append(Environment.NewLine);
                    }
                }
            });

            HexData = hex.ToString();
            AsciiData = ascii.ToString();
        }
    }
}
