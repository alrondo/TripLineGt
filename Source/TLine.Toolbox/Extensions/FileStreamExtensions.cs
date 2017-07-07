using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace TripLine.Toolbox.Extensions
{
    public static class FileStreamExtensions
    {
        [DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Ansi)]
        static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, [In] IntPtr lpOverlapped);

        public static unsafe FileStream Write(this FileStream @this, byte* data, long length)
        {
            uint bytesWritten;
            WriteFile(@this.SafeFileHandle, (IntPtr)data, (uint)length, out bytesWritten, IntPtr.Zero);

            return @this;
        }

        public static unsafe void Write(this SafeFileHandle fileHandle, byte* data, long length)
        {
            uint bytesWritten;
            WriteFile(fileHandle, (IntPtr)data, (uint)length, out bytesWritten, IntPtr.Zero);
        }

        public static void Write(this SafeFileHandle fileHandle, IntPtr data, long length)
        {
            uint bytesWritten;
            WriteFile(fileHandle, data, (uint)length, out bytesWritten, IntPtr.Zero);
        }
    }
}
