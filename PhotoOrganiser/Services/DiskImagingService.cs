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
        public double Progress { get; set; }
        public async Task ImageDiskAsync(string sourceDevicePath, string targetFilePath)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var diskImager = new DiskImager(sourceDevicePath, targetFilePath);
            diskImager.ProgressUpdated += (sender, progress) =>
            {
                Progress = progress;
            };
            await Task.Run(() => diskImager.Copy(cancellationTokenSource.Token));
        }
    }
}
