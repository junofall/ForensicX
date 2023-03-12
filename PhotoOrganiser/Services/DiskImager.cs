using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace ForensicX.Services
{
    public class DiskImager
    {
        public string SourceDevicePath { get; set; }
        public string TargetFilePath { get; set; }
        public int BufferSize { get; set; } = 1024 * 1024; // Buffer defaults to 1 MB.

        private CancellationTokenSource _cancellationTokenSource;

        public DiskImager() 
        {

        }

        public void Copy(string sourceDevicePath, string targetFilePath, CancellationToken cancellationToken)
        {
            const int bufferSize = 1024 * 1024; // 1 meg buffer
            string deviceSourcePath = @"\\.\PHYSICALDRIVE3";
            string fileTargetPath = @"F:\SanDisk_AndAnotherOne.001";

            using (var reader = new BinaryReader(new DeviceStream(deviceSourcePath)))
            using (var writer = new BinaryWriter(new FileStream(fileTargetPath, FileMode.Create)))
            {
                var buffer = new byte[bufferSize];
                long streamLength = reader.BaseStream.Length;
                int progress = 0;
                int bytesRead;
                long totalBytesRead = 0;

                Debug.WriteLine($"Source Path: {deviceSourcePath}");
                Debug.WriteLine($"Destination: {fileTargetPath}");
                Debug.WriteLine($"Now copying {streamLength} bytes...");

                using (var md5 = MD5.Create())
                using (var sha1 = SHA1.Create())
                {
                    try
                    {
                        while (totalBytesRead < streamLength)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            bytesRead = reader.Read(buffer, 0, bufferSize);
                            if (bytesRead == 0)
                            {
                                break;
                            }

                            writer.Write(buffer, 0, bytesRead);
                            md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                            sha1.TransformBlock(buffer, 0, bytesRead, buffer, 0);

                            // Update progress
                            long newProgress = totalBytesRead * 100 / streamLength;
                            if (newProgress != progress)
                            {
                                Console.WriteLine($"Progress: {newProgress}%");
                                progress = (int)newProgress;
                            }

                            if (totalBytesRead % (100 * 1024 * 1024) == 0) // flush to disk every 100mb
                            {
                                Console.WriteLine((totalBytesRead / (1024 * 1024)) + " MB copied");
                                writer.Flush();
                            }

                            totalBytesRead += bytesRead;
                        }
                        md5.TransformFinalBlock(buffer, 0, 0);
                        sha1.TransformFinalBlock(buffer, 0, 0);

                        reader.Close();
                        writer.Close();

                        Debug.WriteLine($"Imaging done. {totalBytesRead} bytes written to {fileTargetPath}");

                        byte[] source_md5Hash = md5.Hash;
                        byte[] source_sha1Hash = sha1.Hash;

                        Debug.WriteLine("====Initial Checksums====");
                        Debug.WriteLine($"MD5: {BitConverter.ToString(source_md5Hash).Replace("-", "")}");
                        Debug.WriteLine($"SHA1: {BitConverter.ToString(source_sha1Hash).Replace("-", "")}");

                        // Validate checksums by reading the source again.
                        Debug.WriteLine("Validating Checksums...");
                        using (var targetReader = new BinaryReader(new FileStream(fileTargetPath, FileMode.Open)))
                        {
                            var targetBuffer = new byte[100 * 1024 * 1024]; // 100 MB read buffer
                            long totalTargetBytesRead = 0;

                            using (var validate_md5 = MD5.Create())
                            using (var validate_sha1 = SHA1.Create())
                            {
                                while (totalTargetBytesRead < streamLength)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    int targetBytesRead = targetReader.Read(targetBuffer, 0, targetBuffer.Length);
                                    if (targetBytesRead == 0)
                                    {
                                        break;
                                    }

                                    validate_md5.TransformBlock(targetBuffer, 0, targetBytesRead, targetBuffer, 0);
                                    validate_sha1.TransformBlock(targetBuffer, 0, targetBytesRead, targetBuffer, 0);
                                    totalTargetBytesRead += targetBytesRead;
                                }
                                validate_md5.TransformFinalBlock(targetBuffer, 0, 0);
                                validate_sha1.TransformFinalBlock(targetBuffer, 0, 0);

                                byte[] target_Md5Hash = validate_md5.Hash;
                                byte[] target_Sha1Hash = validate_sha1.Hash;

                                Debug.WriteLine("====Final Checksums====");
                                Debug.WriteLine($"MD5: {BitConverter.ToString(target_Md5Hash).Replace("-", "")}");
                                Debug.WriteLine($"SHA1: {BitConverter.ToString(target_Sha1Hash).Replace("-", "")}");

                                if (Enumerable.SequenceEqual(source_md5Hash, target_Md5Hash) && Enumerable.SequenceEqual(source_sha1Hash, target_Sha1Hash))
                                {
                                    Debug.WriteLine("Good copy! Checksums match!");
                                }
                                else
                                {
                                    Debug.WriteLine("!! BAD COPY !! Checksums do NOT match.");
                                }
                                targetReader.Close();
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        public async Task Start(string sourceDevicePath, string targetFilePath)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => Copy(sourceDevicePath, targetFilePath, _cancellationTokenSource.Token));
        }

        public void Abort()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
