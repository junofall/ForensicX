using ForensicX.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.ViewModels.SubViewModels
{
    public class DiskViewSubViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<DiskSector> _diskSectors;
        public ObservableCollection<DiskSector> DiskSectors
        {
            get => _diskSectors;
            set
            {
                _diskSectors = value;
                OnPropertyChanged();
            }
        }

        public DiskViewSubViewModel()
        {
            long numSectors = 1000000; // Specify the number of sectors you want to create
            Random random = new Random();

            DiskSectors = new ObservableCollection<DiskSector>();

            for (int i = 0; i < numSectors; i++)
            {
                DiskSectors.Add(new DiskSector { IsAllocated = random.Next(2) == 1, SectorNumber = i });
            }
        }
    }
}
