using ForensicX.Models.Disks.FileSystems.FAT16B.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Interfaces
{
    public interface IReadableFileSystem
    {
        void LoadFileEntryData(FileEntry file);

        Task LoadFileEntryDataAsync(FileEntry file);
    }
}
