using CommunityToolkit.Mvvm.ComponentModel;
using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.ViewModels.SubViewModels
{
    public class InfoViewModel : ObservableObject
    {
        private FileEntry? _selectedFile;

        public FileEntry? SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (SetProperty(ref _selectedFile, value))
                {
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(FilePath));
                    OnPropertyChanged(nameof(FileSize));
                    OnPropertyChanged(nameof(CreationTime));
                    OnPropertyChanged(nameof(LastAccessedTime));
                    OnPropertyChanged(nameof(LastModifiedTime));
                    OnPropertyChanged(nameof(Attributes));
                    OnPropertyChanged(nameof(IsDeleted));
                    OnPropertyChanged(nameof(IsReadOnly));
                    OnPropertyChanged(nameof(IsHidden));
                    OnPropertyChanged(nameof(IsDirectory));
                    OnPropertyChanged(nameof(IsSystem));
                    OnPropertyChanged(nameof(IsArchive));
                }
            }
        }

        public string FileName => _selectedFile?.Name ?? string.Empty;

        public string FilePath => _selectedFile?.FilePath ?? string.Empty;

        public string FileSize => _selectedFile?.FileSize.ToString() ?? string.Empty;

        public string CreationTime => _selectedFile?.CreationTime.ToString() ?? string.Empty;

        public string LastAccessedTime => _selectedFile?.LastAccessedTime.ToString() ?? string.Empty;

        public string LastModifiedTime => _selectedFile?.LastModifiedTime.ToString() ?? string.Empty;

        public string Attributes => _selectedFile?.Attributes.ToString() ?? string.Empty;

        public string IsDeleted => _selectedFile?.IsDeleted == true ? "Yes" : "No";

        public string IsReadOnly => _selectedFile?.IsReadOnly == true ? "Yes" : "No";

        public string IsHidden => _selectedFile?.IsHidden == true ? "Yes" : "No";

        public string IsDirectory => _selectedFile?.IsDirectory == true ? "Yes" : "No";

        public string IsSystem => _selectedFile?.IsSystem == true ? "Yes" : "No";

        public string IsArchive => _selectedFile?.IsArchive == true ? "Yes" : "No";

    }
}
