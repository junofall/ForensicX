using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.StartScreen;

namespace ForensicX.Services
{
    public class DiskImager
    {
        private string SourceDevicePath { get; set; }
        private string TargetFilePath { get; set; }
        public ulong CurrentTotalBytesRead { get; set; }
        public ulong CurrentStreamLength { get; set; }

        public event EventHandler<double> ProgressUpdated;
        public event EventHandler<double> ChecksumProgressUpdated;

        private CancellationTokenSource _cancellationTokenSource;

        public DiskImager(string sourceDevicePath, string targetFilePath) 
        {
            SourceDevicePath = sourceDevicePath;
            TargetFilePath = targetFilePath;
        }

        public void Copy(CancellationToken cancellationToken)
        {
            try
            {
                int bufferSizeMultiplier = 512 * 64;
                DeviceStream deviceStream = new DeviceStream(SourceDevicePath);
                int progress = 0;
                Stopwatch sw = new Stopwatch();


                byte[] sourceMd5Hash;
                byte[] sourceSha1Hash;

                using (var reader = new BinaryReader(deviceStream))
                using (var writer = new BinaryWriter(new FileStream(TargetFilePath, FileMode.Create)))
                using (var md5 = MD5.Create())
                using (var sha1 = SHA1.Create())
                {
                    int bufferSize = (int)deviceStream.BytesPerSector;
                    Debug.WriteLine("Bytes Per Sector: " + deviceStream.BytesPerSector);
                    Debug.WriteLine("Disk Size: " + deviceStream.DiskSize);
                    CurrentStreamLength = (ulong)deviceStream.Length;
                    var buffer = new byte[bufferSize * bufferSizeMultiplier];
                    long streamLength = reader.BaseStream.Length;
                    int bytesRead;
                    long totalBytesRead = 0;

                    Debug.WriteLine($"Source Path: {SourceDevicePath}");
                    Debug.WriteLine($"Destination: {TargetFilePath}");
                    Debug.WriteLine($"Now copying {streamLength} bytes...");

                    sw.Start();

                    while (totalBytesRead < streamLength)
                    {
                        long remainingSize = deviceStream.DiskSize - totalBytesRead;
                        int readSize = remainingSize >= bufferSize * bufferSizeMultiplier
                            ? bufferSize * bufferSizeMultiplier
                            : (int)(remainingSize - (remainingSize % bufferSize));

                        bytesRead = reader.Read(buffer, 0, readSize);
                        cancellationToken.ThrowIfCancellationRequested();

                        if (bytesRead == 0)
                        {
                            break;
                        }

                        if (totalBytesRead % (100 * 1024 * 1024) == 0)
                        {
                            //Debug.WriteLine((totalBytesRead / (1024 * 1024)) + " MB copied");
                            writer.Flush();
                        }

                        // Update progress
                        long newProgress = (long)(((double)totalBytesRead / streamLength) * 10000);
                        if (newProgress != progress)
                        {
                            // Calculate time remaining
                            double elapsedSeconds = sw.Elapsed.TotalSeconds;
                            double rate = totalBytesRead / elapsedSeconds; // bytes per second
                            long remainingBytes = streamLength - totalBytesRead;
                            double estimatedSecondsRemaining = remainingBytes / rate;

                            TimeSpan estimatedTimeRemaining = TimeSpan.FromSeconds(estimatedSecondsRemaining);
                            DateTime estimatedCompletionTime = DateTime.Now.Add(estimatedTimeRemaining);

                            Debug.WriteLine($"Progress: {newProgress / 100.0}%, ETA: {estimatedTimeRemaining} ({estimatedCompletionTime})");

                            progress = (int)(newProgress / 100);
                            OnProgressUpdated(progress);
                        }

                        writer.Write(buffer, 0, bytesRead);
                        md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                        sha1.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                        totalBytesRead += bytesRead;
                        CurrentTotalBytesRead = (ulong)totalBytesRead;
                    }

                    md5.TransformFinalBlock(buffer, 0, 0);
                    sha1.TransformFinalBlock(buffer, 0, 0);
                    sourceMd5Hash = md5.Hash;
                    sourceSha1Hash = sha1.Hash;

                    if (progress != 100)
                    {
                        progress = 100;
                        OnProgressUpdated(progress);
                    }

                    reader.Close();
                    writer.Close();

                    Debug.WriteLine($"Imaging done. {totalBytesRead} bytes written to {TargetFilePath}");
                }

                // Reopen file and validate
                CmpSrcDstHashes(cancellationToken, sourceMd5Hash, sourceSha1Hash);
                
            }
            catch(OperationCanceledException oce)
            {
                Debug.WriteLine("Operation Cancelled : " + oce.Message);
            }
        }

        private void CmpSrcDstHashes(CancellationToken cancellationToken, byte[] srcMd5Hash, byte[] srcSha1Hash)
        {
            Stopwatch sw = new Stopwatch();
            Debug.WriteLine("Validating Hashes, this may take a while...");
            byte[] destMd5Hash;
            byte[] destSha1Hash;

            using (var fileStream = new FileStream(TargetFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var md5Dest = MD5.Create())
                using (var sha1Dest = SHA1.Create())
                {
                    byte[] bufferDest = new byte[4 * 1024 * 1024];
                    int bytesReadDest;
                    long totalBytesReadDest = 0;
                    long streamLengthDest = fileStream.Length;
                    int progressDest = 0;

                    sw.Start();
                    while ((bytesReadDest = fileStream.Read(bufferDest, 0, bufferDest.Length)) > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        md5Dest.TransformBlock(bufferDest, 0, bytesReadDest, bufferDest, 0);
                        sha1Dest.TransformBlock(bufferDest, 0, bytesReadDest, bufferDest, 0);

                        totalBytesReadDest += bytesReadDest;

                        // Update progress
                        long newProgressDest = (long)(((double)totalBytesReadDest / streamLengthDest) * 10000);
                        if (newProgressDest != progressDest)
                        {
                            // Calculate time remaining
                            double elapsedSeconds = sw.Elapsed.TotalSeconds;
                            double rate = totalBytesReadDest / elapsedSeconds; // bytes per second
                            long remainingBytes = streamLengthDest - totalBytesReadDest;
                            double estimatedSecondsRemaining = remainingBytes / rate;

                            TimeSpan estimatedTimeRemaining = TimeSpan.FromSeconds(estimatedSecondsRemaining);
                            DateTime estimatedCompletionTime = DateTime.Now.Add(estimatedTimeRemaining);

                            Debug.WriteLine($"Checksum Validation Progress: {newProgressDest / 100.0}%, ETA: {estimatedTimeRemaining} ({estimatedCompletionTime})");

                            progressDest = (int)(newProgressDest / 100);
                            OnChecksumProgressUpdated(progressDest);
                        }
                    }

                    md5Dest.TransformFinalBlock(bufferDest, 0, 0);
                    sha1Dest.TransformFinalBlock(bufferDest, 0, 0);
                    destMd5Hash = md5Dest.Hash;
                    destSha1Hash = sha1Dest.Hash;
                }
            }

            bool md5Match = StructuralComparisons.StructuralEqualityComparer.Equals(srcMd5Hash, destMd5Hash);
            bool sha1Match = StructuralComparisons.StructuralEqualityComparer.Equals(srcSha1Hash, destSha1Hash);

            Debug.WriteLine($"MD5 Checksums match: {md5Match}");
            Debug.WriteLine($"SHA1 Checksums match: {sha1Match}");
        }

        private void OnProgressUpdated(double progress)
        {
            ProgressUpdated?.Invoke(this, progress);
        }

        private void OnChecksumProgressUpdated(double progress)
        {
            ChecksumProgressUpdated?.Invoke(this, progress);
        }

        public async Task Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => Copy(_cancellationTokenSource.Token));
        }

        public void Abort()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
