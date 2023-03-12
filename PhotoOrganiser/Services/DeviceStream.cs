using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

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


        // disk geometry hierarchy

        // bytes per sector
        // sectors per track
        // tracks per cylinder
        // sectors per cylinder

        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY_EX
        {
            public ulong BytesPerSector;
            public ulong SectorsPerTrack;
            public uint TracksPerCylinder;
            public uint Cylinders;
            public long DiskSize;
            public byte[] Data;
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
            IntPtr lpOverlapped);

        private SafeFileHandle handleValue = null;
        private FileStream _fs = null;
        private readonly long length = 0;

        public DeviceStream(string device)
        {
            Load(device);

            // Retrieve disk geometry information
            DISK_GEOMETRY_EX geometry = new();
            bool success = DeviceIoControl(handleValue, IOCTL_DISK_GET_DRIVE_GEOMETRY_EX, IntPtr.Zero, 0, ref geometry, Marshal.SizeOf(geometry), out int bytesReturned, IntPtr.Zero);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            length = geometry.DiskSize;
        }

        private void Load(string Path)
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentNullException(nameof(Path));
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
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
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