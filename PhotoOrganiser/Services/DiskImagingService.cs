using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicX.Services
{
    public class DiskImagingService
    {
        public event EventHandler<double> ProgressUpdated;

        public async Task ImageDiskAsync(string sourceDevicePath, string targetFilePath)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var diskImager = new DiskImager(sourceDevicePath, targetFilePath);
            await Task.Run(() => diskImager.Copy(cancellationTokenSource.Token));
        }
    }
}
