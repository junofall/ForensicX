using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Text;

namespace ForensicX.Services
{
    public class DeviceStream : Stream, IDisposable
    {
        // Control code declarations from the Win32 API headers.
        public const short FILE_ATTRIBUTE_NORMAL = 0x80;
        public const short INVALID_HANDLE_VALUE = -1;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;

        private const uint IOCTL_DISK_GET_DRIVE_GEOMETRY_EX = 0x700a0;


        public ulong BytesPerSector { get; private set; }
        public long DiskSize { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY_EX
        {
            public DISK_GEOMETRY Geometry;
            public long DiskSize;
            public byte[] Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY
        {
            public long Cylinders;
            public MEDIA_TYPE MediaType;
            public uint TracksPerCylinder;
            public uint SectorsPerTrack;
            public uint BytesPerSector;
        }

        public enum MEDIA_TYPE
        {
            Unknown,
            F5_1Pt2_512,
            F3_1Pt44_512,
            F3_2Pt88_512,
            F3_20Pt8_512,
            F3_720_512,
            F5_360_512,
            F5_320_512,
            F5_320_1024,
            F5_180_512,
            F5_160_512,
            RemovableMedia,
            FixedMedia,
            F3_120M_512,
            F3_640_512,
            F5_640_512,
            F5_720_512,
            F3_1Pt2_512,
            F3_1Pt23_1024,
            F5_1Pt23_1024,
            F3_128Mb_512,
            F3_230Mb_512,
            F8_256_128,
            F3_200Mb_512,
            F3_240M_512,
            F3_32M_512
        }

        // Use interop to call the CreateFile function.
        // For more information about CreateFile,
        // see the unmanaged MSDN reference library.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
          uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
          uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(
            IntPtr hFile,                        // handle to file
            byte[] lpBuffer,                // data buffer
            int nNumberOfBytesToRead,        // number of bytes to read
            ref int lpNumberOfBytesRead,    // number of bytes read
            IntPtr lpOverlapped
            //
            // ref OVERLAPPED lpOverlapped        // overlapped buffer
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            ref DISK_GEOMETRY_EX lpOutBuffer,
            int nOutBufferSize,
            out int lpBytesReturned,
            IntPtr lpOverlapped
            );

        private SafeFileHandle handleValue = null;
        private FileStream _fs = null;
        private long length = 0;

        public DeviceStream(string device)
        {
            Load(device);

            // Retrieve disk geometry information
            DISK_GEOMETRY_EX geometry = new DISK_GEOMETRY_EX();
            int bytesReturned = 0;
            bool success = DeviceIoControl(handleValue, IOCTL_DISK_GET_DRIVE_GEOMETRY_EX, IntPtr.Zero, 0, ref geometry, Marshal.SizeOf(typeof(DISK_GEOMETRY_EX)), out bytesReturned, IntPtr.Zero);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            length = geometry.DiskSize;
            BytesPerSector = geometry.Geometry.BytesPerSector;
            DiskSize = geometry.DiskSize;
        }

        private void Load(string Path)
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentNullException("Path");
            }

            // Try to open the file.
            IntPtr ptr = CreateFile(Path, GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            handleValue = new SafeFileHandle(ptr, true);
            _fs = new FileStream(handleValue, FileAccess.Read);

            // If the handle is invalid,
            // get the last Win32 error 
            // and throw a Win32Exception.
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            return;
        }

        public override long Length => length;

        public override long Position
        {
            get => _fs.Position;
            set => _fs.Position = value;
        }
        /// <summary>
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and 
        /// (offset + count - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int BytesRead = 0;
            var BufBytes = new byte[count];
            if (!ReadFile(handleValue.DangerousGetHandle(), BufBytes, count, ref BytesRead, IntPtr.Zero))
            {
                int errorCode = Marshal.GetHRForLastWin32Error();
                Debug.WriteLine($"Error reading sector at position {Position}: 0x{errorCode:X}");
                Debug.WriteLine($"BufBytes: {BufBytes.Length} ; count: {count} ; count/sec {count / 512} ; BytesRead: {BytesRead}");
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                // Log the error and the current position

            }
            for (int i = 0; i < BytesRead; i++)
            {
                buffer[offset + i] = BufBytes[i];
            }
            return BytesRead;
        }
        public override int ReadByte()
        {
            int BytesRead = 0;
            var lpBuffer = new byte[1];
            if (!ReadFile(
            handleValue.DangerousGetHandle(),                        // handle to file
            lpBuffer,                // data buffer
            1,        // number of bytes to read
            ref BytesRead,    // number of bytes read
            IntPtr.Zero
            ))
            { Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); ; }
            return lpBuffer[0];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            handleValue.Close();
            handleValue.Dispose();
            handleValue = null;
            base.Close();
        }
        private bool disposed = false;

        new void Dispose()
        {
            Dispose(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        private new void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (handleValue != null)
                    {
                        _fs.Dispose();
                        handleValue.Close();
                        handleValue.Dispose();
                        handleValue = null;
                    }
                }
                // Note disposing has been done.
                disposed = true;

            }
        }

    }
}